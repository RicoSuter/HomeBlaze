using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Namotion.Trackable.Model;
using Namotion.Trackable.Utilities;

namespace Namotion.Trackable;

public class TrackableContext<TObject> : ITrackableContext, IObservable<TrackedPropertyChange>
    where TObject : class
{
    private readonly Subject<TrackedPropertyChange> _changesSubject = new();
    private readonly ConcurrentDictionary<PropertyInfo, PropertyReflectionMetadata> _propertyReflectionMetadataCache = new();

    // TODO: Switch to concurrent dict
    private readonly Dictionary<ITrackable, Tracker> _trackers = new();
    private readonly ITrackableFactory _trackableFactory;

    public TObject Object { get; private set; }

    object ITrackableContext.Object => Object;

    public IReadOnlyCollection<Tracker> AllTrackers
    {
        get
        {
            lock (_trackers)
            {
                return _trackers.Values;
            }
        }
    }

    public IReadOnlyCollection<TrackedProperty> AllProperties
    {
        get
        {
            lock (_trackers)
            {
                return _trackers
                    .Values
                    .SelectMany(t => t.Properties)
                    .ToArray();
            }
        }
    }

    public TrackableContext(ITrackableFactory trackableFactory)
    {
        _trackableFactory = trackableFactory;

        var proxy = CreateProxy<TObject>();
        if (Object == null)
        {
            Object = proxy;
            InitializeProxy((ITrackable)proxy);
        }
    }

    public TProxy CreateProxy<TProxy>()
    {
        return _trackableFactory.CreateProxy<TProxy>();
    }

    public object CreateProxy(Type proxyType)
    {
        return _trackableFactory.CreateProxy(proxyType);
    }

    internal void InitializeProxy(ITrackable proxy)
    {
        Object = (TObject)proxy;
        AttachTrackable(proxy, null, null);
    }

    public IDisposable Subscribe(IObserver<TrackedPropertyChange> observer)
        => _changesSubject.Subscribe(observer);

    public IObservable<TField?> Observe<TField>(Expression<Func<TObject, TField>> selector)
    {
        var targetPath = selector.GetExpressionPath();
        var function = selector.Compile();
        return this
            .Select(change => change.Property.Path.StartsWith(targetPath) && change.Value is not null)
            .Select(_ => function.Invoke(Object));
    }

    internal void AttachPropertyValue(TrackedProperty property, object newValue)
    {
        foreach ((var trackable, var index) in FindNewTrackables(newValue))
        {
            AttachTrackable(trackable, property, index);
        }
    }

    private void AttachTrackable(ITrackable proxy, TrackedProperty? parentProperty, object? parentCollectionKey)
    {
        var tracker = TryGetTracker(proxy);
        if (tracker == null)
        {
            if (parentProperty != null && proxy is ITrackableWithParent group)
            {
                group.Parent = parentProperty.Parent.Object;
            }

            tracker = new Tracker(proxy, parentProperty, parentCollectionKey);
        
            lock (_trackers)
            {
                _trackers[proxy] = tracker;
                if (parentProperty != null)
                {
                    ((HashSet<Tracker>)parentProperty.Children).Add(tracker);
                }
            }

            tracker.Object.AddTrackableContext(this);

            // create tracker for children
            foreach (var propertyInfo in proxy
                .GetType()
                .BaseType! // get properties from actual type
                .GetProperties())
            {
                var propertyReflectionMetadata = _propertyReflectionMetadataCache.GetOrAdd(propertyInfo, pi => new PropertyReflectionMetadata(pi));
                if (propertyReflectionMetadata.IsTrackable)
                {
                    var property = CreateAndAddTrackableProperty(propertyReflectionMetadata, tracker, parentCollectionKey);
                    if (property.IsReadable)
                    {
                        var value = property.GetValue();
                        if (value != null)
                        {
                            AttachPropertyValue(property, value);
                        }
                    }
                }
            }

            tracker.FreezeProperties();
        }
    }

    internal void DetachPropertyValue(TrackedProperty property, object? newValue)
    {
        lock (_trackers)
        {
            var newTrackables = newValue != null ? new HashSet<ITrackable>(
                FindNewTrackables(newValue).Select(t => t.trackable)) : null;

            foreach (var tracker in property.Children.Where(c => newTrackables?.Contains(c.Object) != true))
            {
                _trackers.Remove(tracker.Object);
                ((HashSet<Tracker>)property.Children).Remove(tracker);
                tracker.Object.RemoveTrackableContext(this);

                foreach (var childProperty in tracker.Properties)
                {
                    DetachPropertyValue(childProperty, null);
                }
            }
        }
    }

    private IEnumerable<(ITrackable trackable, object? index)> FindNewTrackables(object? newValue)
    {
        if (newValue is IDictionary newDictionary)
        {
            foreach (var key in newDictionary.Keys)
            {
                var value = newDictionary[key];
                if (value is ITrackable trackable)
                {
                    yield return (trackable, key);
                }
            }
        }
        else if (newValue is ICollection newTrackables)
        {
            var index = 0;
            foreach (var trackable in newTrackables.OfType<ITrackable>())
            {
                yield return (trackable, index);
                index++;
            }
        }
        else if (newValue is ITrackable trackable)
        {
            yield return (trackable, null);
        }
    }

    public Tracker? TryGetTracker(object proxy)
    {
        lock (_trackers)
        {
            return _trackers.TryGetValue((ITrackable)proxy, out var tracker) ? tracker : null;
        }
    }

    internal void MarkPropertyAsChanged(TrackedProperty variable)
    {
        MarkVariableAsChanged(variable, new HashSet<TrackedProperty>());
    }

    private void MarkVariableAsChanged(TrackedProperty property, HashSet<TrackedProperty> markedVariables)
    {
        _changesSubject.OnNext(new TrackedPropertyChange(property, property.Data, property.LastValue));

        markedVariables.Add(property);

        foreach (var dependentProperty in property.UsedByProperties)
        {
            if (!markedVariables.Contains(dependentProperty))
            {
                MarkVariableAsChanged(dependentProperty, markedVariables);
            }
        }
    }

    private TrackedProperty CreateAndAddTrackableProperty(PropertyReflectionMetadata propertyReflectionMetadata, Tracker parent, object? parentCollectionKey)
    {
        propertyReflectionMetadata.EnsureIsVirtual();

        var property = propertyReflectionMetadata.CreateProperty(parent);
        parent.AddProperty(property);

        foreach (var attribute in propertyReflectionMetadata.PropertyInitializers)
        {
            attribute.InitializeProperty(property, parent, parentCollectionKey, this);
        }

        TryInitializeRequiredProperty(propertyReflectionMetadata, parent);
        return property;
    }

    private void TryInitializeRequiredProperty(PropertyReflectionMetadata propertyReflectionMetadata, Tracker parent)
    {
        if (propertyReflectionMetadata.IsRequired)
        {
            var child = CreateProxy(propertyReflectionMetadata.PropertyType);
            propertyReflectionMetadata.SetValue(parent.Object, child);
        }
    }

    void ITrackableContext.InitializeProxy(ITrackable proxy) => InitializeProxy(proxy);

    void ITrackableContext.AttachPropertyValue(TrackedProperty property, object newValue) => AttachPropertyValue(property, newValue);

    void ITrackableContext.DetachPropertyValue(TrackedProperty property, object? newValue) => DetachPropertyValue(property, newValue);

    void ITrackableContext.MarkPropertyAsChanged(TrackedProperty property) => MarkPropertyAsChanged(property);
}

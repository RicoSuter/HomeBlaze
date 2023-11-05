using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.Model;
using Namotion.Trackable.Utilities;

namespace Namotion.Trackable;

public class TrackableContext<TObject> : ITrackableContext, IObservable<TrackedPropertyChange>
    where TObject : class
{
    private readonly Subject<TrackedPropertyChange> _changesSubject = new();
    private readonly ConcurrentDictionary<PropertyInfo, object[]> _propertyInfoAttributesCache = new();

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
        if (newValue is IDictionary newDictionary)
        {
            foreach (var key in newDictionary.Keys)
            {
                var value = newDictionary[key];
                if (value is ITrackable trackable)
                {
                    AttachTrackable(trackable, property, key);
                }
            }

            //if (newDictionary is INotifyCollectionChanged notifyCollectionChanged)
            //{
            //    notifyCollectionChanged.CollectionChanged += OnCollectionChanged;
            //}
        }
        else if (newValue is ICollection newTrackables)
        {
            var index = 0;
            foreach (var child in newTrackables.OfType<ITrackable>())
            {
                AttachTrackable(child, property, index);
                index++;
            }
        }
        else if (newValue is ITrackable trackable)
        {
            AttachTrackable(trackable, property, null);
        }
    }

    internal void DetachPropertyValue(TrackedProperty property)
    {
        lock (_trackers)
        {
            foreach (var tuple in _trackers.Where(t => t.Value.ParentProperty == property))
            {
                tuple.Key.RemoveTrackableContext(this);
                _trackers.Remove(tuple.Key);

                foreach (var childProperty in tuple.Value.Properties)
                {
                    DetachPropertyValue(childProperty);
                }
            }
        }
    }

    private void AttachTrackable(ITrackable proxy, TrackedProperty? parentProperty, object? parentCollectionKey)
    {
        var tracker = TryGetTracker(proxy);
        if (tracker == null)
        {
            var parentPath = parentProperty != null ?
                (parentProperty.Path + (parentCollectionKey != null ? $"[{parentCollectionKey}]" : string.Empty)) :
                string.Empty;

            if (parentProperty != null && proxy is ITrackableWithParent group)
            {
                group.Parent = parentProperty.Parent.Object;
            }

            tracker = new Tracker(proxy, parentPath, parentProperty, parentCollectionKey);
            lock (_trackers)
            {
                _trackers[proxy] = tracker;
            }

            tracker.Object.AddTrackableContext(this);

            // create tracker for children
            foreach (var propertyInfo in proxy
                .GetType()
                .BaseType! // get properties from actual type
                .GetProperties())
            {
                var attributes = _propertyInfoAttributesCache.GetOrAdd(propertyInfo, pi => pi.GetCustomAttributes(true));
                var attribute = attributes.OfType<TrackableAttribute>().FirstOrDefault();
                if (attribute != null)
                {
                    var property = CreateAndAddTrackableProperty(propertyInfo, attribute, attributes, tracker, parentCollectionKey);
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

    //private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    //{
    //    if (e.NewItems is not null)
    //    {
    //        foreach (var trackable in e.NewItems.OfType<ITrackable>())
    //        {
    //            Attach(trackable, null, null); // TODO: get parent somehow
    //        }
    //    }

    //    if (e.OldItems is not null)
    //    {
    //        foreach (var trackable in e.OldItems.OfType<ITrackable>())
    //        {
    //            Detach(trackable);
    //        }
    //    }
    //}

    public Tracker? TryGetTracker(object proxy)
    {
        lock (_trackers)
        {
            return _trackers.TryGetValue((ITrackable)proxy, out var tracker) ? tracker : null;
        }
    }

    internal void MarkPropertyAsChanged(TrackedProperty variable)
    {
        MarkVariableAsChanged(variable, AllProperties, new HashSet<TrackedProperty>());
    }

    private void MarkVariableAsChanged(TrackedProperty property, IReadOnlyCollection<TrackedProperty> allProperties, HashSet<TrackedProperty> markedVariables)
    {
        _changesSubject.OnNext(new TrackedPropertyChange(property, property.Data, property.GetValue()));

        markedVariables.Add(property);

        foreach (var dependentVariable in allProperties
            .Where(v => v.DependentProperties?.Contains(property) == true && !markedVariables.Contains(v)))
        {
            MarkVariableAsChanged(dependentVariable, allProperties, markedVariables);
        }
    }

    private TrackedProperty CreateAndAddTrackableProperty(PropertyInfo propertyInfo, TrackableAttribute trackableAttribute, object[] attributes, Tracker parent, object? parentCollectionKey)
    {
        if (propertyInfo.GetMethod?.IsVirtual == false ||
            propertyInfo.SetMethod?.IsVirtual == false)
        {
            throw new InvalidOperationException($"Trackable property {propertyInfo.DeclaringType?.Name}.{propertyInfo.Name} must be virtual.");
        }

        var property = trackableAttribute.CreateProperty(propertyInfo, parent);
        parent.AddProperty(property);

        foreach (var attribute in attributes.OfType<ITrackablePropertyInitializer>())
        {
            attribute.InitializeProperty(property, parent, parentCollectionKey, this);
        }

        TryInitializeRequiredProperty(propertyInfo, attributes, parent);
        return property;
    }

    private void TryInitializeRequiredProperty(PropertyInfo propertyInfo, object[] attributes, Tracker parent)
    {
        if (attributes.Any(a => a is RequiredAttribute || a.GetType().FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute") &&
            propertyInfo.PropertyType.IsClass &&
            propertyInfo.PropertyType.FullName?.StartsWith("System.") == false)
        {
            var child = CreateProxy(propertyInfo.PropertyType);
            propertyInfo.SetValue(parent.Object, child);
        }
    }

    void ITrackableContext.InitializeProxy(ITrackable proxy) => InitializeProxy(proxy);

    void ITrackableContext.AttachPropertyValue(TrackedProperty property, object newValue) => AttachPropertyValue(property, newValue);

    void ITrackableContext.DetachPropertyValue(TrackedProperty property) => DetachPropertyValue(property);

    void ITrackableContext.MarkPropertyAsChanged(TrackedProperty property) => MarkPropertyAsChanged(property);
}

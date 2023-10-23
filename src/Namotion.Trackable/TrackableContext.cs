using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

using Namotion.Trackable.Attributes;
using Namotion.Trackable.Model;
using Namotion.Trackable.Utilities;
using Namotion.Trackable.Validation;

namespace Namotion.Trackable;

public class TrackableContext<TObject> : ITrackableContext, ITrackableFactory, IObservable<TrackedPropertyChange>
    where TObject : class
{
    private readonly object _lock = new();

    private readonly Subject<TrackedPropertyChange> _changesSubject = new Subject<TrackedPropertyChange>();

    private readonly TrackableInterceptor _interceptor;
    private readonly IEnumerable<IInterceptor> _interceptors;
    private readonly IServiceProvider _serviceProvider;

    private readonly HashSet<Tracker> _trackers = new();

    public TObject Object { get; private set; }

    object ITrackableContext.Object => Object;

    public IEnumerable<TrackedProperty> AllProperties
    {
        get
        {
            lock (_lock)
            {
                return _trackers
                    .SelectMany(t => t.Properties)
                    .ToArray();
            }
        }
    }

    public TrackableContext(
        IEnumerable<ITrackablePropertyValidator> propertyValidators,
        IEnumerable<ITrackableInterceptor> interceptors,
        IServiceProvider serviceProvider)
    {
        _interceptor = new TrackableInterceptor(propertyValidators, this);
        _interceptors = interceptors;
        _serviceProvider = serviceProvider;

        var proxy = (TObject)serviceProvider
            .CreateProxy(typeof(TObject), this, interceptors
                .Concat(new[] { _interceptor })
                .ToArray());

        if (Object == null)
        {
            Object = proxy;
            Initialize(proxy);
        }
    }

    internal void Initialize(object proxy)
    {
        Object = (TObject)proxy;
        Attach(Object, null, null);
    }

    public TChild CreateProxy<TChild>()
    {
        return (TChild)_serviceProvider.CreateProxy(typeof(TChild), this, _interceptors
            .Concat(new[] { _interceptor })
            .ToArray());
    }

    public object CreateProxy(Type trackableType)
    {
        return _serviceProvider.CreateProxy(trackableType, this, _interceptors
            .Concat(new[] { _interceptor })
            .ToArray());
    }

    public IDisposable Subscribe(IObserver<TrackedPropertyChange> observer)
        => _changesSubject.Subscribe(observer);

    public IObservable<TField?> Observe<TField>(Expression<Func<TObject, TField>> selector)
    {
        var targetPath = selector.GetFullExpressionPath();
        var function = selector.Compile();
        return this
            .Select(change => change.Property.Path.StartsWith(targetPath) && change.Value is not null)
            .Select(_ => function.Invoke(Object));
    }

    internal void Attach(TrackedProperty property, object newValue)
    {
        if (newValue is ICollection newTrackables)
        {
            var index = 0;
            foreach (var child in newTrackables.OfType<ITrackable>())
            {
                Attach(child, property, index);
                index++;
            }
        }
        else
        {
            Attach(newValue, property, null);
        }
    }

    private Tracker? TryGetTracker(object proxy)
    {
        lock (_lock)
        {
            return _trackers.SingleOrDefault(t => t.Object == proxy);
        }
    }

    private void Attach(object proxy, TrackedProperty? parentProperty, int? parentCollectionIndex)
    {
        var parentPath =
            parentProperty != null ? (
                parentProperty.AbsolutePath +
                (parentCollectionIndex != null ? $"[{parentCollectionIndex}]" : string.Empty)) : string
            .Empty;

        if (TryGetTracker(proxy) == null)
        {
            CreateTracker(proxy, parentPath, parentProperty, parentCollectionIndex);
        }
    }

    internal void Detach(object previousValue)
    {
        lock (_lock)
        {
            if (previousValue is ICollection previousTrackables)
            {
                foreach (var child in previousTrackables.OfType<ITrackable>())
                {
                    Detach(child);
                }
            }
            else if (previousValue is ITrackable trackable)
            {
                // TODO: Call RemoveTrackableContext on all trackables (also children)

                trackable.RemoveTrackableContext(this);
                _trackers.RemoveWhere(t => t.Object != previousValue);
            }
        }
    }

    internal void MarkVariableAsChanged(TrackedProperty variable)
    {
        MarkVariableAsChanged(variable, new HashSet<TrackedProperty>());
    }

    private void MarkVariableAsChanged(TrackedProperty property, HashSet<TrackedProperty> changedVariables)
    {
        _changesSubject.OnNext(new TrackedPropertyChange(property,
            new Dictionary<string, object?>(property.Data),
            property.GetValue()));

        changedVariables.Add(property);

        foreach (var dependentVariable in AllProperties
            .Where(v => v.DependentProperties.Contains(property) &&
                        !changedVariables.Contains(v))
            .ToArray())
        {
            MarkVariableAsChanged(dependentVariable, changedVariables);
        }
    }

    public void CreateTracker(object proxy, string parentPath, TrackedProperty? parentProperty, int? parentCollectionIndex)
    {
        var tracker = TryGetTracker(proxy);
        if (tracker == null)
        {
            if (parentProperty != null && proxy is ITrackableWithParent group)
            {
                group.Parent = parentProperty.Parent.Object;
            }

            tracker = new Tracker(proxy, parentPath, parentProperty, this);
            lock (_lock)
            {
                _trackers.Add(tracker);
            }

            ((ITrackable)tracker.Object).AddTrackableContext(this);

            // create tracker for children
            foreach (var property in proxy
                .GetType()
                .BaseType! // get properties from actual type
                .GetProperties()
                .Where(p => p.GetMethod?.IsVirtual == true || p.SetMethod?.IsVirtual == true))
            {
                var trackableAttribute = property.GetCustomAttribute<TrackableAttribute>(true);
                if (trackableAttribute != null)
                {
                    trackableAttribute.CreateTrackableProperty(property, tracker, parentCollectionIndex);
                }
            }

            // initialize derived property dependencies
            foreach (var stateProperty in tracker.Properties
                .Where(p => p.SetMethod == null))
            {
                stateProperty.GetValue();
            }
        }

        // find child trackables (created in ctor)
        if (parentProperty != null)
        {
            foreach (var stateProperty in tracker.Properties
                .Where(p => p.SetMethod != null && p.GetMethod != null))
            {
                var value = stateProperty.GetValue();
                if (value is ITrackable || value is ICollection)
                {
                    Attach(stateProperty, value);
                }
            }
        }
    }

    void ITrackableContext.InitializeProxy(object proxy) => Initialize(proxy);

    void ITrackableContext.Attach(TrackedProperty property, object newValue) => Attach(property, newValue);

    void ITrackableContext.Detach(object previousValue) => Detach(previousValue);

    void ITrackableContext.MarkVariableAsChanged(TrackedProperty property) => MarkVariableAsChanged(property);
}

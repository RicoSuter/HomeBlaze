using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using Castle.DynamicProxy;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.Model;
using Namotion.Trackable.Utilities;

namespace Namotion.Trackable;

public class TrackableContext<TObject> : ITrackableContext, IObservable<TrackedPropertyChange>
    where TObject : class
{
    private readonly object _lock = new();

    private readonly Subject<TrackedPropertyChange> _changesSubject = new Subject<TrackedPropertyChange>();

    private readonly HashSet<Tracker> _trackers = new();
    private readonly IEnumerable<ITrackableInterceptor> _interceptors;
    private readonly IServiceProvider _serviceProvider;

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

    public TrackableContext(IEnumerable<ITrackableInterceptor> interceptors, IServiceProvider serviceProvider)
    {
        _interceptors = interceptors;
        _serviceProvider = serviceProvider;

        var proxy = CreateProxy<TObject>();
        if (Object == null)
        {
            Object = proxy;
            Initialize((ITrackable)proxy);
        }
    }

    public TChild CreateProxy<TChild>()
    {
        return (TChild)CreateProxy(_serviceProvider, typeof(TChild));
    }

    public object CreateProxy(Type trackableType)
    {
        return CreateProxy(_serviceProvider, trackableType);
    }

    private object CreateProxy(IServiceProvider serviceProvider, Type proxyType)
    {
        var constructorArguments = proxyType
            .GetConstructors()
            .First()
            .GetParameters()
            .Select(p => p.ParameterType.IsAssignableTo(typeof(ITrackableFactory)) ? this :
                         serviceProvider.GetService(p.ParameterType))
            .ToArray();

        var proxy = new ProxyGenerator()
            .CreateClassProxy(
                proxyType,
                new Type[] { typeof(ITrackable) },
                new ProxyGenerationOptions(),
                constructorArguments,
                new TrackableInterceptor(_interceptors));

        return proxy;
    }

    internal void Initialize(ITrackable proxy)
    {
        Object = (TObject)proxy;
        Attach(proxy, null, null);
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
        if (newValue is IDictionary newDictionary)
        {
            foreach (var key in newDictionary.Keys)
            {
                var value = newDictionary[key];
                if (value is ITrackable trackable)
                {
                    Attach(trackable, property, key);
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
                Attach(child, property, index);
                index++;
            }
        }
        else if (newValue is ITrackable trackable)
        {
            Attach(trackable, property, null);
        }
    }

    private void Attach(ITrackable proxy, TrackedProperty? parentProperty, object? parentCollectionKey)
    {
        var parentPath =
            parentProperty != null ? (
                parentProperty.AbsolutePath +
                (parentCollectionKey != null ? $"[{parentCollectionKey}]" : string.Empty)) : string
            .Empty;

        if (TryGetTracker(proxy) == null)
        {
            CreateTracker(proxy, parentPath, parentProperty, parentCollectionKey);
        }
    }

    internal void Detach(object previousValue)
    {
        lock (_lock)
        {
            if (previousValue is IDictionary newDictionary)
            {
                foreach (var key in newDictionary.Keys)
                {
                    var value = newDictionary[key];
                    if (value is ITrackable trackable)
                    {
                        Detach(trackable);
                    }
                }
            }
            else if (previousValue is ICollection previousTrackables)
            {
                foreach (var child in previousTrackables.OfType<ITrackable>())
                {
                    Detach(child);
                }

                //if (previousTrackables is INotifyCollectionChanged notifyCollectionChanged)
                //{
                //    notifyCollectionChanged.CollectionChanged -= OnCollectionChanged;
                //}
            }
            else if (previousValue is ITrackable trackable)
            {
                // TODO: Call RemoveTrackableContext on all trackables (also children)

                trackable.RemoveTrackableContext(this);
                _trackers.RemoveWhere(t => t.Object != previousValue);
            }
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

    private Tracker? TryGetTracker(object proxy)
    {
        lock (_lock)
        {
            return _trackers.SingleOrDefault(t => t.Object == proxy);
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

    private void CreateTracker(ITrackable proxy, string parentPath, TrackedProperty? parentProperty, object? parentCollectionKey)
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

            tracker.Object.AddTrackableContext(this);

            // create tracker for children
            foreach (var propertyInfo in proxy
                .GetType()
                .BaseType! // get properties from actual type
                .GetProperties())
            {
                var trackableAttribute = propertyInfo.GetCustomAttribute<TrackableAttribute>(true);
                if (trackableAttribute != null)
                {
                    var property = CreateTrackableProperty(trackableAttribute, propertyInfo, tracker, parentCollectionKey);
                    if (property.GetMethod != null)
                    {
                        var value = property.GetValue();
                        if (value != null)
                        {
                            Attach(property, value);
                        }
                    }
                }
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

    private TrackedProperty CreateTrackableProperty(TrackableAttribute trackableAttribute, PropertyInfo propertyInfo, Tracker parent, object? parentCollectionKey)
    {
        if (propertyInfo.GetMethod?.IsVirtual == false || propertyInfo.SetMethod?.IsVirtual == false)
        {
            throw new InvalidOperationException($"Trackable property {propertyInfo.DeclaringType?.Name}.{propertyInfo.Name} must be virtual.");
        }

        var property = trackableAttribute.CreateTrackableProperty(propertyInfo, parent, parentCollectionKey);
        parent.Properties.Add(property);

        foreach (var attribute in propertyInfo.GetCustomAttributes(true).OfType<ITrackableAttribute>())
        {
            attribute.ProcessProperty(property, parent, parentCollectionKey);
        }

        return property;
    }

    void ITrackableContext.InitializeProxy(ITrackable proxy) => Initialize(proxy);

    void ITrackableContext.Attach(TrackedProperty property, object newValue) => Attach(property, newValue);

    void ITrackableContext.Detach(object previousValue) => Detach(previousValue);

    void ITrackableContext.MarkVariableAsChanged(TrackedProperty property) => MarkVariableAsChanged(property);
}

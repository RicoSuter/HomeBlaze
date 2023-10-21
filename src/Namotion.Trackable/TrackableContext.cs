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

public class TrackableContext<TObject> : ITrackableContext, ITrackableFactory, IObservable<TrackablePropertyChange>
    where TObject : class
{
    private readonly Subject<TrackablePropertyChange> _changesSubject = new Subject<TrackablePropertyChange>();

    private readonly TrackableInterceptor _interceptor;
    private readonly IEnumerable<IInterceptor> _interceptors;
    private readonly IServiceProvider _serviceProvider;

    private Tracker[] _trackers = Array.Empty<Tracker>();

    public TObject Object { get; private set; }

    public IReadOnlyCollection<Tracker> Trackables => _trackers;

    object ITrackableContext.Object => Object;

    public IEnumerable<TrackedProperty> AllProperties => _trackers.SelectMany(t => t.Properties);

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
            Initialize(Object);
        }
    }

    internal void Initialize(object obj)
    {
        Object = (TObject)obj;
        Attach(Object, null, null);
    }

    public TChild Create<TChild>()
    {
        return (TChild)_serviceProvider.CreateProxy(typeof(TChild), this, _interceptors
            .Concat(new[] { _interceptor })
            .ToArray());
    }

    public object Create(Type trackableType)
    {
        return _serviceProvider.CreateProxy(trackableType, this, _interceptors
            .Concat(new[] { _interceptor })
            .ToArray());
    }

    public IDisposable Subscribe(IObserver<TrackablePropertyChange> observer)
        => _changesSubject.Subscribe(observer);

    public IObservable<TField?> Observe<TField>(Expression<Func<TObject, TField>> selector)
    {
        var targetPath = GetFullExpressionPath(selector);
        return this.Select(change => change.Property.Path.StartsWith(targetPath) && change.Value is not null)
            .Select(_ => selector.Compile().Invoke(Object));
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

    private void Attach(object proxy, TrackedProperty? parentProperty, int? parentCollectionIndex)
    {
        var parentPath =
            parentProperty != null ? (
                parentProperty.AbsolutePath + 
                (parentCollectionIndex != null ? $"[{parentCollectionIndex}]" : string.Empty)) : string
            .Empty;

        var newTrackables = CreateTrackables(proxy, parentPath, parentProperty, parentCollectionIndex)
            .ToArray();

        foreach (var thing in newTrackables
            .Select(t => t.Object)
            .OfType<ITrackable>())
        {
            thing.AddThingContext(this);
        }

        _trackers = _trackers
            .Concat(newTrackables)
            .ToArray();

        // initialize derived property dependencies
        foreach (var stateProperty in newTrackables
            .SelectMany(t => t.Properties)
            .Where(p => p.SetMethod == null))
        {
            stateProperty.GetValue();
        }
    }

    internal void Detach(object previousValue)
    {
        if (previousValue is ICollection previousTrackables)
        {
            foreach (var child in previousTrackables.OfType<ITrackable>())
            {
                Detach(child);
            }
        }
        else
        {
            // TODO: Call RemoveThingContext
            _trackers = _trackers
                .Where(t => t.Object != previousValue)
                .ToArray();
        }
    }

    internal void MarkVariableAsChanged(TrackedProperty variable)
    {
        MarkVariableAsChanged(variable, new HashSet<TrackedProperty>());
    }

    private void MarkVariableAsChanged(TrackedProperty property, HashSet<TrackedProperty> changedVariables)
    {
        _changesSubject.OnNext(new TrackablePropertyChange(property, new Dictionary<string, object?>(property.Data), property.GetValue()));
        changedVariables.Add(property);

        foreach (var dependentVariable in AllProperties
            .Where(v => v.DependentProperties.Contains(property) &&
                        !changedVariables.Contains(v))
            .ToArray())
        {
            MarkVariableAsChanged(dependentVariable, changedVariables);
        }
    }

    private static string GetFullExpressionPath<TItem, TField>(Expression<Func<TItem, TField>> fieldSelector)
    {
        var parts = new List<string>();
        var body = fieldSelector.Body;
        while (body is MemberExpression member)
        {
            parts.Add(member.Member.Name);
            body = member.Expression;
        }

        parts.Reverse();
        return string.Join(".", parts);
    }

    // TODO: make internal
    public IEnumerable<Tracker> CreateTrackables(object proxy, string parentPath, TrackedProperty? parent, int? parentCollectionIndex)
    {
        if (parent != null && proxy is ITrackableWithParent group)
        {
            group.Parent = parent.Parent.Object;
        }

        var trackable = new Tracker(proxy, parentPath, parent);
        foreach (var property in proxy.GetType()
            .BaseType! // get properties from actual type
            .GetProperties()
            .Where(p => p.GetMethod?.IsVirtual == true || p.SetMethod?.IsVirtual == true))
        {
            var trackableAttribute = property.GetCustomAttribute<TrackableAttribute>(true);
            if (trackableAttribute != null)
            {
                foreach (var child in trackableAttribute.CreateTrackerForProperty(property, trackable, parentCollectionIndex, this))
                {
                    yield return child;
                };
            }
        }
        yield return trackable;
    }

    void ITrackableContext.Initialize(object obj) => Initialize(obj);

    void ITrackableContext.Attach(TrackedProperty property, object newValue) => Attach(property, newValue);

    void ITrackableContext.Detach(object previousValue) => Detach(previousValue);

    void ITrackableContext.MarkVariableAsChanged(TrackedProperty setVariable) => MarkVariableAsChanged(setVariable);
}

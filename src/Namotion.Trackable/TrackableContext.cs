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

    private Model.Trackable[] _trackables = Array.Empty<Model.Trackable>();

    public TObject Object { get; private set; }

    public IReadOnlyCollection<Model.Trackable> Trackables => _trackables;

    object ITrackableContext.Object => Object;

    public IEnumerable<TrackableProperty> AllProperties => _trackables.SelectMany(t => t.Properties);

    public TrackableContext(
        IEnumerable<ITrackablePropertyValidator> propertyValidators, 
        IEnumerable<ITrackableInterceptor> interceptors,
        IServiceProvider serviceProvider)
    {
        _interceptor = new TrackableInterceptor(propertyValidators, this);
        _interceptors = interceptors;
        _serviceProvider = serviceProvider;

        var thing = (TObject)serviceProvider
            .CreateProxy(typeof(TObject), this, interceptors
                .Concat(new[] { _interceptor })
                .ToArray());

        if (Object == null)
        {
            Object = thing;
            Initialize(Object);
        }
    }

    internal void Initialize(object obj)
    {
        Object = (TObject)obj;
        Attach(Object, string.Empty, null, null);
    }

    public TChildTing Create<TChildTing>()
    {
        return (TChildTing)_serviceProvider.CreateProxy(typeof(TChildTing), this, _interceptors
            .Concat(new[] { _interceptor })
            .ToArray());
    }

    public object Create(Type thingType)
    {
        return _serviceProvider.CreateProxy(thingType, this, _interceptors
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

    internal void Attach(TrackableProperty property, object newValue)
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

    private void Attach(object proxy, TrackableProperty parentproperty, int? parentCollectionIndex)
    {
        var path = parentproperty.AbsolutePath + 
            (parentCollectionIndex != null ? $"[{parentCollectionIndex}]" : string.Empty);

        Attach(proxy, path, parentproperty, parentCollectionIndex);
    }

    private void Attach(object proxy, string parentPath, TrackableProperty? parentProperty, int? parentCollectionIndex)
    {
        var newTrackables = CreateTrackables(proxy, parentPath, parentProperty, parentCollectionIndex)
            .ToArray();

        foreach (var thing in newTrackables
            .Select(t => t.Object)
            .OfType<ITrackable>())
        {
            thing.AddThingContext(this);
        }

        _trackables = _trackables
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
            _trackables = _trackables
                .Where(t => t.Object != previousValue)
                .ToArray();
        }
    }

    internal void MarkVariableAsChanged(TrackableProperty variable)
    {
        MarkVariableAsChanged(variable, new HashSet<TrackableProperty>());
    }

    private void MarkVariableAsChanged(TrackableProperty property, HashSet<TrackableProperty> changedVariables)
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
    public IEnumerable<Model.Trackable> CreateTrackables(object proxy, string parentPath, TrackableProperty? parent, int? parentCollectionIndex)
    {
        if (parent != null && proxy is ITrackableWithParent group)
        {
            group.Parent = parent.Parent.Object;
        }

        var trackable = new Model.Trackable(proxy, parentPath, parent);
        foreach (var property in proxy.GetType()
            .BaseType! // get properties from actual type
            .GetProperties()
            .Where(p => p.GetMethod?.IsVirtual == true || p.SetMethod?.IsVirtual == true))
        {
            var trackableAttribute = property.GetCustomAttribute<TrackableAttribute>(true);
            if (trackableAttribute != null)
            {
                foreach (var child in trackableAttribute.CreateTrackablesForProperty(property, this, trackable, parentCollectionIndex))
                {
                    yield return child;
                };
            }
        }
        yield return trackable;
    }

    void ITrackableContext.Initialize(object obj)
    {
        Initialize(obj);
    }

    void ITrackableContext.Attach(TrackableProperty property, object newValue)
    {
        Attach(property, newValue);
    }

    void ITrackableContext.Detach(object previousValue)
    {
        Detach(previousValue);
    }

    void ITrackableContext.MarkVariableAsChanged(TrackableProperty setVariable)
    {
        MarkVariableAsChanged(setVariable);
    }
}

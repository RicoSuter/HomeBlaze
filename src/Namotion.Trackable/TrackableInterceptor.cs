using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Castle.DynamicProxy;
using Namotion.Trackable.Model;

namespace Namotion.Trackable;

public partial class TrackableInterceptor : IInterceptor
{
    private readonly object _lock = new();
    private readonly IEnumerable<ITrackableInterceptor> _interceptors;
    private readonly IInterceptor[] _castleInterceptors;

    private ITrackableContext[] _trackableContexts = Array.Empty<ITrackableContext>();

    public IEnumerable<ITrackableContext> Contexts => _trackableContexts;

    [ThreadStatic]
    private static Stack<List<TrackedProperty>>? _touchedProperties;

    public TrackableInterceptor(IEnumerable<ITrackableInterceptor> interceptors)
    {
        _interceptors = interceptors;
        _castleInterceptors = _interceptors.OfType<IInterceptor>().Reverse().ToArray();
    }

    public void Intercept(IInvocation invocation)
    {
        if (invocation.Method?.Name == nameof(ITrackable.AddTrackableContext) &&
            invocation.Method.DeclaringType?.IsAssignableTo(typeof(ITrackable)) == true)
        {
            lock (_lock)
            {
                _trackableContexts = _trackableContexts.Append((ITrackableContext)invocation.Arguments[0]).ToArray();
            }
            return;
        }
        else if (invocation.Method?.Name == nameof(ITrackable.RemoveTrackableContext) &&
                 invocation.Method.DeclaringType?.IsAssignableTo(typeof(ITrackable)) == true)
        {
            lock (_lock)
            {
                _trackableContexts = _trackableContexts.Where(c => c != (ITrackableContext)invocation.Arguments[0]).ToArray();
            }
            return;
        }
        else if (invocation.Method?.Name == $"get_{nameof(ITrackable.Interceptor)}" &&
                 invocation.Method.DeclaringType?.IsAssignableTo(typeof(ITrackable)) == true)
        {
            invocation.ReturnValue = this;
            return;
        }

        if (invocation.Method?.Name.StartsWith("get_") == true)
        {
            OnReadProperty(invocation);
        }
        else if (invocation.Method?.Name.StartsWith("set_") == true)
        {
            OnWriteProperty(invocation);
        }
        else
        {
            invocation.Proceed();
        }
    }

    private void OnReadProperty(IInvocation invocation)
    {
        var trackableContexts = _trackableContexts;
        var propertyName = invocation.Method.Name.Substring(4);

        foreach (var trackableContext in trackableContexts)
        {
            if (invocation.InvocationTarget is ITrackable trackable)
            {
                if (trackableContext.Object == null)
                {
                    trackableContext.InitializeProxy(trackable);
                }
            }

            var property = trackableContext.TryGetTrackedProperty(invocation.InvocationTarget, propertyName);
            if (property != null)
            {
                OnBeforeReadProperty(property, trackableContext);
            }
        }

        invocation.Proceed();

        foreach (var trackableContext in trackableContexts)
        {
            var property = trackableContext.TryGetTrackedProperty(invocation.InvocationTarget, propertyName);
            if (property != null)
            {
                OnAfterReadProperty(property, invocation.ReturnValue, trackableContext);
            }
        }
    }

    private void OnBeforeReadProperty(TrackedProperty property, ITrackableContext trackableContext)
    {
        if (property.IsDerived)
        {
            if (_touchedProperties == null)
            {
                _touchedProperties = new Stack<List<TrackedProperty>>();
            }

            _touchedProperties.Push(new List<TrackedProperty>());
        }

        foreach (var interceptor in _interceptors)
        {
            interceptor.OnBeforeReadProperty(property, trackableContext);
        }
    }

    private void OnAfterReadProperty(TrackedProperty property, object? newValue, ITrackableContext trackableContext)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.OnAfterReadProperty(property, newValue, trackableContext);
        }

        if (property.IsDerived)
        {
            var result = _touchedProperties?.Pop();
            property.DependentProperties = result?.ToArray() ?? Array.Empty<TrackedProperty>();
        }

        if (_touchedProperties?.Any() == true)
        {
            _touchedProperties.Peek().Add(property);
        }
    }

    private void OnWriteProperty(IInvocation invocation)
    {
        var trackableContexts = _trackableContexts;

        var propertyName = invocation.Method.Name.Substring(4);
      
        var newValue = invocation.Arguments[0];
        var previousValue = invocation.Method.DeclaringType?
            .GetProperty(propertyName)?
            //.ToContextualProperty() // TODO: Check performance
            .GetValue(invocation.InvocationTarget, null);

        foreach (var trackableContext in trackableContexts)
        {
            if (invocation.InvocationTarget is ITrackable trackable)
            {
                if (trackableContext.Object == null)
                {
                    trackableContext.InitializeProxy(trackable);
                }
            }

            var property = trackableContext.TryGetTrackedProperty(invocation.InvocationTarget, propertyName);
            if (property != null)
            {
                OnBeforeWriteProperty(property, newValue, previousValue, trackableContext);
            }
        }

        invocation.Proceed();

        foreach (var trackableContext in trackableContexts)
        {
            var property = trackableContext.TryGetTrackedProperty(invocation.InvocationTarget, propertyName);
            if (property != null)
            {
                OnAfterWriteProperty(property, newValue, previousValue, trackableContext);
            }
        }
    }

    private void OnBeforeWriteProperty(TrackedProperty property, object? newValue, object? previousValue, ITrackableContext trackableContext)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.OnBeforeWriteProperty(property, newValue, previousValue, trackableContext);
        }
    }

    private void OnAfterWriteProperty(TrackedProperty property, object? newValue, object? previousValue, ITrackableContext trackableContext)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.OnAfterWriteProperty(property, newValue, previousValue, trackableContext);
        }

        if (!Equals(previousValue, newValue))
        {
            if (previousValue != null && (previousValue is ITrackable || previousValue is ICollection))
            {
                trackableContext.DetachPropertyValue(property, previousValue);
            }

            if (newValue != null && (newValue is ITrackable || newValue is ICollection))
            {
                trackableContext.AttachPropertyValue(property, newValue);
            }
        }

        trackableContext.MarkPropertyAsChanged(property);
    }
}

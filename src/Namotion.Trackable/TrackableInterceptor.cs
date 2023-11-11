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

            var property = trackableContext.TryGetTracker(invocation.InvocationTarget)?.TryGetProperty(propertyName);
            if (property != null)
            {
                OnBeforeReadProperty(property, trackableContext);
            }
        }

        try
        {
            invocation.Proceed();
        }
        finally
        {
            foreach (var trackableContext in trackableContexts)
            {
                var property = trackableContext.TryGetTracker(invocation.InvocationTarget)?.TryGetProperty(propertyName);
                if (property != null)
                {
                    property.LastKnownValue = invocation.ReturnValue;
                    OnAfterReadProperty(property, invocation.ReturnValue, trackableContext);
                }
            }
        }
    }

    private void OnBeforeReadProperty(TrackedProperty property, ITrackableContext trackableContext)
    {
        if (property.IsDerived)
        {
            property.OnBeforeRead();
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

        property.OnAfterRead();
    }

    private void OnWriteProperty(IInvocation invocation)
    {
        var trackableContexts = _trackableContexts;
        var propertyName = invocation.Method.Name.Substring(4);
        var newValue = invocation.Arguments[0];

        foreach (var trackableContext in trackableContexts)
        {
            if (invocation.InvocationTarget is ITrackable trackable)
            {
                if (trackableContext.Object == null)
                {
                    trackableContext.InitializeProxy(trackable);
                }
            }

            var property = trackableContext.TryGetTracker(invocation.InvocationTarget)?.TryGetProperty(propertyName);
            if (property != null)
            {
                OnBeforeWriteProperty(property, newValue, property.LastKnownValue, trackableContext);
            }
        }

        invocation.Proceed();

        foreach (var trackableContext in trackableContexts)
        {
            var property = trackableContext.TryGetTracker(invocation.InvocationTarget)?.TryGetProperty(propertyName);
            if (property != null)
            {
                var previousValue = property.LastKnownValue;

                property.LastKnownValue = newValue;
                OnAfterWriteProperty(property, newValue, previousValue, trackableContext);
                property.RaisePropertyChanged();
            }
        }
    }

    private void OnBeforeWriteProperty(TrackedProperty property, object? newValue, object? currentValue, ITrackableContext trackableContext)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.OnBeforeWriteProperty(property, newValue, currentValue, trackableContext);
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
                trackableContext.DetachPropertyValue(property, newValue);
            }

            if (newValue != null && (newValue is ITrackable || newValue is ICollection))
            {
                trackableContext.AttachPropertyValue(property, newValue);
            }
        }
    }
}

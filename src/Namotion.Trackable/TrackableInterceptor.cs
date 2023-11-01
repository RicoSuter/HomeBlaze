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
    private readonly ICollection<ITrackableContext> _trackableContexts = new HashSet<ITrackableContext>();
    private readonly IEnumerable<ITrackableInterceptor> _interceptors;
    private readonly IInterceptor[] _castleInterceptors;

    public IEnumerable<ITrackableContext> Contexts => _trackableContexts;

    [ThreadStatic]
    private static Stack<Tuple<TrackedProperty, List<TrackedProperty>>>? _touchedProperties;

    public TrackableInterceptor(IEnumerable<ITrackableInterceptor> interceptors)
    {
        _interceptors = interceptors;
        _castleInterceptors = _interceptors.OfType<IInterceptor>().Reverse().ToArray();
    }

    public void Intercept(IInvocation invocation)
    {
        lock (_lock)
        {
            if (invocation.Method?.Name == nameof(ITrackable.AddTrackableContext) &&
                invocation.Method.DeclaringType?.IsAssignableTo(typeof(ITrackable)) == true)
            {
                _trackableContexts.Add((ITrackableContext)invocation.Arguments[0]);
                return;
            }
            else if (invocation.Method?.Name == nameof(ITrackable.RemoveTrackableContext) &&
                     invocation.Method.DeclaringType?.IsAssignableTo(typeof(ITrackable)) == true)
            {
                _trackableContexts.Remove((ITrackableContext)invocation.Arguments[0]);
                return;
            }
            else if (invocation.Method?.Name == $"get_{nameof(ITrackable.Interceptor)}" &&
                     invocation.Method.DeclaringType?.IsAssignableTo(typeof(ITrackable)) == true)
            {
                invocation.ReturnValue = this;
                return;
            }
        }

        if (_castleInterceptors.Any())
        {
            invocation = new InvocationInterceptor(invocation, ProceedInternal);

            foreach (var interceptor in _castleInterceptors)
            {
                invocation = new InvocationInterceptor(invocation, interceptor.Intercept);
            }

            invocation.Proceed();
        }
        else
        {
            ProceedInternal(invocation);
        }
    }

    private void ProceedInternal(IInvocation invocation)
    {
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
        ITrackableContext[] trackableContexts;
        lock (_lock)
        {
            trackableContexts = _trackableContexts.ToArray();
        }

        foreach (var interceptor in _interceptors)
        {
            if (interceptor.OnReadProperty(invocation) == false)
            {
                return;
            }
        }

        foreach (var trackableContext in trackableContexts)
        {
            if (invocation.InvocationTarget is ITrackable trackable)
            {
                if (trackableContext.Object == null)
                {
                    trackableContext.InitializeProxy(trackable);
                }
            }

            var getProperty = trackableContext
                .AllProperties
                .FirstOrDefault(v => v.Parent.Object == invocation.InvocationTarget &&
                                     v.GetMethod?.Name == invocation.Method?.Name);

            if (getProperty != null)
            {
                OnBeforeReadProperty(getProperty, trackableContext);
            }
        }

        invocation.Proceed();

        foreach (var trackableContext in trackableContexts)
        {
            var getProperty = trackableContext
                .AllProperties
                .FirstOrDefault(v => v.Parent.Object == invocation.InvocationTarget &&
                                     v.GetMethod?.Name == invocation.Method?.Name);

            if (getProperty != null)
            {
                OnAfterReadProperty(getProperty, invocation.ReturnValue, trackableContext);
            }
        }
    }

    private void OnBeforeReadProperty(TrackedProperty getProperty, ITrackableContext trackableContext)
    {
        if (_touchedProperties == null)
        {
            _touchedProperties = new Stack<Tuple<TrackedProperty, List<TrackedProperty>>>();
        }

        if (getProperty.IsDerived)
        {
            _touchedProperties!.Push(new Tuple<TrackedProperty, List<TrackedProperty>>(getProperty, new List<TrackedProperty>()));
        }

        foreach (var interceptor in _interceptors)
        {
            interceptor.OnBeforeReadProperty(getProperty, trackableContext);
        }
    }

    private void OnAfterReadProperty(TrackedProperty getProperty, object? newValue, ITrackableContext trackableContext)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.OnAfterReadProperty(getProperty, newValue, trackableContext);
        }

        if (_touchedProperties == null)
        {
            _touchedProperties = new Stack<Tuple<TrackedProperty, List<TrackedProperty>>>();
        }

        if (getProperty.IsDerived)
        {
            var result = _touchedProperties.Pop();
            getProperty.DependentProperties = result.Item2.ToArray();
        }

        if (_touchedProperties.Any())
        {
            _touchedProperties.Peek().Item2.Add(getProperty);
        }
    }

    private void OnWriteProperty(IInvocation invocation)
    {
        foreach (var interceptor in _interceptors)
        {
            if (interceptor.OnWriteProperty(invocation) == false)
            {
                return;
            }
        }

        ITrackableContext[] trackableContexts;
        lock (_lock)
        {
            trackableContexts = _trackableContexts.ToArray();
        }

        var methodName = invocation.Method.Name.Replace("set_", "get_");
        var previousValue = invocation.InvocationTarget.GetType().GetMethod(methodName)?.Invoke(invocation.InvocationTarget, null);
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

            var setProperty = trackableContext
                .AllProperties
                .FirstOrDefault(v => v.Parent.Object == invocation.InvocationTarget &&
                                     v.SetMethod?.Name == invocation.Method?.Name);

            if (setProperty != null)
            {
                OnBeforeWriteProperty(setProperty, newValue, previousValue, trackableContext);
            }
        }

        invocation.Proceed();

        foreach (var trackableContext in trackableContexts)
        {
            var setProperty = trackableContext
                .AllProperties
                .FirstOrDefault(v => v.Parent.Object == invocation.InvocationTarget &&
                                     v.SetMethod?.Name == invocation.Method?.Name);

            if (setProperty != null)
            {
                OnAfterWriteProperty(setProperty, newValue, previousValue, trackableContext);
            }
        }
    }

    private void OnBeforeWriteProperty(TrackedProperty setProperty, object? newValue, object? previousValue, ITrackableContext trackableContext)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.OnBeforeWriteProperty(setProperty, newValue, previousValue, trackableContext);
        }
    }

    private void OnAfterWriteProperty(TrackedProperty setProperty, object? newValue, object? previousValue, ITrackableContext trackableContext)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.OnAfterWriteProperty(setProperty, newValue, previousValue, trackableContext);
        }

        if (!Equals(previousValue, newValue))
        {
            if (previousValue != null && (previousValue is ITrackable || previousValue is ICollection))
            {
                trackableContext.DetachPropertyValue(setProperty, previousValue);
            }

            if (newValue != null && (newValue is ITrackable || newValue is ICollection))
            {
                trackableContext.AttachPropertyValue(setProperty, newValue);
            }
        }

        trackableContext.MarkPropertyAsChanged(setProperty);
    }
}

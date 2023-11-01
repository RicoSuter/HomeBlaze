using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

using Castle.DynamicProxy;
using Namotion.Trackable.Model;

namespace Namotion.Trackable;

public class TrackableInterceptor : IInterceptor
{
    private readonly object _lock = new();
    private readonly ICollection<ITrackableContext> _trackableContexts = new HashSet<ITrackableContext>();

    private readonly IEnumerable<ITrackableInterceptor> _interceptors;

    [ThreadStatic]
    private static Stack<Tuple<TrackedProperty, List<TrackedProperty>>>? _touchedProperties;

    public TrackableInterceptor(IEnumerable<ITrackableInterceptor> interceptors)
    {
        _interceptors = interceptors;
    }

    public void Intercept(IInvocation invocation)
    {
        ITrackableContext[] trackableContexts;

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

            trackableContexts = _trackableContexts.ToArray();
        }

        if (invocation.Method?.Name.StartsWith("get_") == true)
        {
            HandlePropertyGetter(invocation);
        }
        else if (invocation.Method?.Name.StartsWith("set_") == true)
        {
            HandlePropertySetter(invocation);
        }
        else
        {
            invocation.Proceed();
        }
    }

    private void HandlePropertyGetter(IInvocation invocation)
    {
        foreach (var trackableContext in _trackableContexts)
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
                OnBeforePropertyRead(getProperty, trackableContext);
            }
        }

        invocation.Proceed();

        foreach (var trackableContext in _trackableContexts)
        {
            var getProperty = trackableContext
                .AllProperties
                .FirstOrDefault(v => v.Parent.Object == invocation.InvocationTarget &&
                                     v.GetMethod?.Name == invocation.Method?.Name);

            if (getProperty != null)
            {
                OnAfterPropertyRead(getProperty, invocation.ReturnValue, trackableContext);
            }
        }
    }

    private void OnBeforePropertyRead(TrackedProperty getProperty, ITrackableContext trackableContext)
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
            interceptor.OnBeforePropertyRead(getProperty, trackableContext);
        }
    }

    private void OnAfterPropertyRead(TrackedProperty getProperty, object? newValue, ITrackableContext trackableContext)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.OnAfterPropertyRead(getProperty, newValue, trackableContext);
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

    private void HandlePropertySetter(IInvocation invocation)
    {
        var methodName = invocation.Method.Name.Replace("set_", "get_");
        var previousValue = invocation.InvocationTarget.GetType().GetMethod(methodName)?.Invoke(invocation.InvocationTarget, null);
        var newValue = invocation.Arguments[0];

        foreach (var trackableContext in _trackableContexts)
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
                OnBeforePropertyWrite(setProperty, newValue, previousValue, trackableContext);
            }
        }

        invocation.Proceed();

        foreach (var trackableContext in _trackableContexts)
        {
            var setProperty = trackableContext
                .AllProperties
                .FirstOrDefault(v => v.Parent.Object == invocation.InvocationTarget &&
                                     v.SetMethod?.Name == invocation.Method?.Name);

            if (setProperty != null)
            {
                OnAfterPropertyWrite(setProperty, newValue, previousValue, trackableContext);
            }
        }
    }

    private void OnBeforePropertyWrite(TrackedProperty setProperty, object? newValue, object? previousValue, ITrackableContext trackableContext)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.OnBeforePropertyWrite(setProperty, newValue, previousValue, trackableContext);
        }
    }

    private void OnAfterPropertyWrite(TrackedProperty setProperty, object? newValue, object? previousValue, ITrackableContext trackableContext)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.OnAfterPropertyWrite(setProperty, newValue, previousValue, trackableContext);
        }

        if (!Equals(previousValue, newValue))
        {
            if (previousValue != null && (previousValue is ITrackable || previousValue is ICollection))
            {
                trackableContext.Detach(previousValue);
            }

            if (newValue != null && (newValue is ITrackable || newValue is ICollection))
            {
                trackableContext.Attach(setProperty, newValue);
            }
        }

        trackableContext.MarkVariableAsChanged(setProperty);
    }
}

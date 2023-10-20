using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Linq;

using Castle.DynamicProxy;

using Namotion.Trackable.Model;
using Namotion.Trackable.Validation;

namespace Namotion.Trackable;

public class TrackableInterceptor : ITrackableInterceptor
{
    private ICollection<ITrackableContext> _trackableContexts = new HashSet<ITrackableContext>();

    [ThreadStatic]
    private static Stack<Tuple<TrackableProperty, List<TrackableProperty>>>? _touchedProperties;

    private readonly IEnumerable<ITrackablePropertyValidator> _propertyValidators;

    public TrackableInterceptor(IEnumerable<ITrackablePropertyValidator> propertyValidators, ITrackableContext trackableContext)
    {
        _propertyValidators = propertyValidators;
        _trackableContexts.Add(trackableContext);
    }

    public void Intercept(IInvocation invocation)
    {
        ITrackableContext[] trackableContexts;
        lock (_trackableContexts)
        {
            if (invocation.Method?.Name == nameof(ITrackable.AddThingContext) &&
                invocation.Method.DeclaringType?.IsAssignableTo(typeof(ITrackable)) == true)
            {
                _trackableContexts.Add((ITrackableContext)invocation.Arguments[0]);
                return;
            }
            else if (invocation.Method?.Name == nameof(ITrackable.RemoveThingContext) &&
                     invocation.Method.DeclaringType?.IsAssignableTo(typeof(ITrackable)) == true)
            {
                _trackableContexts.Remove((ITrackableContext)invocation.Arguments[0]);
                return;
            }

            trackableContexts = _trackableContexts.ToArray();
        }

        foreach (var thingContext in _trackableContexts)
        {
            if (thingContext.Object == null &&
                invocation.InvocationTarget is ITrackable trackable)
            {
                thingContext.Initialize(trackable);
            }

            var getProperty = thingContext
                .AllProperties
                .FirstOrDefault(v => v.Parent.Object == invocation.InvocationTarget &&
                                     v.GetMethod?.Name == invocation.Method?.Name);
            var setProperty = thingContext
                .AllProperties
                .FirstOrDefault(v => v.Parent.Object == invocation.InvocationTarget &&
                                     v.SetMethod?.Name == invocation.Method?.Name);

            if (setProperty != null)
            {
                var errors = _propertyValidators
                    .SelectMany(v => v.Validate(setProperty, invocation.Arguments[0], thingContext))
                    .ToArray();

                if (errors.Any())
                {
                    throw new ValidationException(string.Join("\n", errors.Select(e => e.ErrorMessage)));
                }

                var previousValue = setProperty.GetValue();
                var newValue = invocation.Arguments[0];
                if (!Equals(previousValue, newValue))
                {
                    invocation.Proceed();

                    if (previousValue != null && (previousValue is ITrackable || previousValue is ICollection))
                    {
                        thingContext.Detach(previousValue);
                    }

                    if (newValue != null && (newValue is ITrackable || newValue is ICollection))
                    {
                        thingContext.Attach(setProperty, newValue);
                    }                    
                }
            }
            else if (getProperty != null)
            {
                if (_touchedProperties == null)
                {
                    _touchedProperties = new Stack<Tuple<TrackableProperty, List<TrackableProperty>>>();
                }

                if (getProperty.IsDerived)
                {
                    var dependencies = new List<TrackableProperty>();

                    _touchedProperties!.Push(new Tuple<TrackableProperty, List<TrackableProperty>>(getProperty, dependencies));

                    invocation.Proceed();
                    getProperty.DependentProperties = dependencies.ToArray();

                    _touchedProperties.Pop();
                }
                else
                {
                    invocation.Proceed();
                }

                if (_touchedProperties.Any())
                {
                    _touchedProperties.Peek().Item2.Add(getProperty);
                }
            }
            else
            {
                invocation.Proceed();
            }

            if (setProperty != null)
            {
                thingContext.MarkVariableAsChanged(setProperty);
            }
        }
    }
}

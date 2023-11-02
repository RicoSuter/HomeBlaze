using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Linq;
using Castle.DynamicProxy;
using Namotion.Trackable.Model;

namespace Namotion.Trackable.Validation;

public class ValidationTrackableInterceptor : ITrackableInterceptor, IInterceptor
{
    private readonly IEnumerable<ITrackablePropertyValidator> _propertyValidators;

    public ValidationTrackableInterceptor(IEnumerable<ITrackablePropertyValidator> propertyValidators)
    {
        _propertyValidators = propertyValidators;
    }

    public void Intercept(IInvocation invocation)
    {
        invocation.Proceed();
    }

    public void OnBeforeWriteProperty(TrackedProperty property, object? newValue, object? previousValue, ITrackableContext trackableContext)
    {
        var errors = _propertyValidators
           .SelectMany(v => v.Validate(property, newValue, trackableContext))
           .ToArray();

        if (errors.Any())
        {
            throw new ValidationException(string.Join("\n", errors.Select(e => e.ErrorMessage)));
        }
    }
}

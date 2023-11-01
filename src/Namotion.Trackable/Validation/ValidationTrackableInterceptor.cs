using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reactive.Linq;
using Namotion.Trackable.Model;

namespace Namotion.Trackable.Validation;

public class ValidationTrackableInterceptor : ITrackableInterceptor
{
    private readonly IEnumerable<ITrackablePropertyValidator> _propertyValidators;

    public ValidationTrackableInterceptor(IEnumerable<ITrackablePropertyValidator> propertyValidators)
    {
        _propertyValidators = propertyValidators;
    }

    public void OnAfterPropertyWrite(TrackedProperty setProperty, object? newValue, object? previousValue, ITrackableContext trackableContext)
    {
        var errors = _propertyValidators
           .SelectMany(v => v.Validate(setProperty, newValue, trackableContext))
           .ToArray();

        if (errors.Any())
        {
            throw new ValidationException(string.Join("\n", errors.Select(e => e.ErrorMessage)));
        }
    }
}

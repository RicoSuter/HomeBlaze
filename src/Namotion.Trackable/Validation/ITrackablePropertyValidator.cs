using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Namotion.Trackable.Model;

namespace Namotion.Trackable.Validation;

public interface ITrackablePropertyValidator
{
    IEnumerable<ValidationResult> Validate(TrackedProperty property, object? value, ITrackableContext context);
}


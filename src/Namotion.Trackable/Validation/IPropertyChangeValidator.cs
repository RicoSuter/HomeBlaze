using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Namotion.Trackable.Model;

namespace Namotion.Trackable.Validation;

public interface IPropertyChangeValidator
{
    IEnumerable<ValidationResult> Validate(TrackableProperty property, object? newValue, ITrackableContext context);
}

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Namotion.Trackable.Model;

namespace Namotion.Trackable;

public interface ITrackablePropertyValidator
{
    IEnumerable<ValidationResult> Validate(TrackableProperty property, object? value, ITrackableContext context);
}


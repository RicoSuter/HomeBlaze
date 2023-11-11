using Namotion.Trackable.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Namotion.Trackable.Validation
{
    public class DataAnnotationsValidator : ITrackablePropertyValidator
    {
        public IEnumerable<ValidationResult> Validate(TrackedProperty property, object? value, ITrackableContext context)
        {
            var results = new List<ValidationResult>();

            if (value is not null)
            {
                var validationContext = new ValidationContext(((ProxyTracker)property.Parent).Object)
                {
                    MemberName = property.Name
                };

                Validator.TryValidateProperty(value, validationContext, results);
            }

            return results;
        }
    }
}

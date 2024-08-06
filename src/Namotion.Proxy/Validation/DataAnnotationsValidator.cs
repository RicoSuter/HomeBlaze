using System.ComponentModel.DataAnnotations;

namespace Namotion.Proxy.Validation;

public class DataAnnotationsValidator : IProxyPropertyValidator
{
    public IEnumerable<ValidationResult> Validate(ProxyPropertyReference property, object? value, IProxyContext context)
    {
        var results = new List<ValidationResult>();

        if (value is not null)
        {
            var validationContext = new ValidationContext(property.Proxy)
            {
                MemberName = property.Name
            };

            Validator.TryValidateProperty(value, validationContext, results);
        }

        return results;
    }
}

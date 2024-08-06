using Namotion.Proxy.Abstractions;

using System.ComponentModel.DataAnnotations;
using System.Reactive.Linq;

namespace Namotion.Proxy.Validation;

public class ProxyValidationHandler : IProxyWriteHandler
{
    private readonly Lazy<IProxyPropertyValidator[]> _propertyValidators;

    public ProxyValidationHandler(Lazy<IProxyPropertyValidator[]> propertyValidators)
    {
        _propertyValidators = propertyValidators;
    }

    public void WriteProperty(ProxyPropertyWriteContext context, Action<ProxyPropertyWriteContext> next)
    {
        var errors = _propertyValidators.Value
            .SelectMany(v => v.Validate(context.Property, context.NewValue, context.Context))
            .ToArray();

        if (errors.Any())
        {
            throw new ValidationException(string.Join("\n", errors.Select(e => e.ErrorMessage)));
        }

        next(context);
    }
}

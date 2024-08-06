using Namotion.Proxy.Abstractions;

using System.ComponentModel.DataAnnotations;

namespace Namotion.Proxy.Validation;

public interface IProxyPropertyValidator : IProxyHandler
{
    IEnumerable<ValidationResult> Validate(ProxyPropertyReference property, object? value, IProxyContext context);
}


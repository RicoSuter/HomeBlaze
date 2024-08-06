namespace Namotion.Proxy.Abstractions;

public record struct ProxyPropertyWriteContext(
    ProxyPropertyReference Property,
    object? CurrentValue,
    object? NewValue,
    bool IsDerived,
    IProxyContext Context)
{
}

namespace Namotion.Proxy.Abstractions;

public record struct ProxyPropertyChanged(
    ProxyPropertyReference Property,
    object? OldValue,
    object? NewValue,
    IProxyContext Context)
{
}

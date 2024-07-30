namespace Namotion.Proxy.Abstractions;

public record struct ProxyLifecycleContext(
    ProxyPropertyReference Property,
    object? Index,
    IProxy Proxy,
    int ReferenceCount,
    IProxyContext Context)
{
}

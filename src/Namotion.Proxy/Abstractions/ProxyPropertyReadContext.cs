namespace Namotion.Proxy.Abstractions;

public record struct ProxyPropertyReadContext(
    ProxyPropertyReference Property,
    IProxyContext Context)
{
}

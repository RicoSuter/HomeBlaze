namespace Namotion.Proxy.Abstractions;

public interface IProxyReadHandler : IProxyHandler
{
    object? ReadProperty(ProxyPropertyReadContext context, Func<ProxyPropertyReadContext, object?> next);
}

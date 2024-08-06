namespace Namotion.Proxy.Abstractions;

public interface IProxyWriteHandler : IProxyHandler
{
    void WriteProperty(ProxyPropertyWriteContext context, Action<ProxyPropertyWriteContext> next);
}

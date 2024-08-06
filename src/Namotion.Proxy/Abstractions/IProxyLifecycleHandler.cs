namespace Namotion.Proxy.Abstractions;

public interface IProxyLifecycleHandler : IProxyHandler
{
    public void OnProxyAttached(ProxyLifecycleContext context);

    public void OnProxyDetached(ProxyLifecycleContext context);
}

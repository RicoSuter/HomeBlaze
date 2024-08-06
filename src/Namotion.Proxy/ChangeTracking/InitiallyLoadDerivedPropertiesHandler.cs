using Namotion.Proxy.Abstractions;

namespace Namotion.Proxy.ChangeTracking;

internal class InitiallyLoadDerivedPropertiesHandler : IProxyLifecycleHandler
{
    public void OnProxyAttached(ProxyLifecycleContext context)
    {
        foreach (var property in context.Proxy.Properties.Where(p => p.Value.IsDerived))
        {
            property.Value.GetValue?.Invoke(context.Proxy);
        }
    }

    public void OnProxyDetached(ProxyLifecycleContext context)
    {
    }
}

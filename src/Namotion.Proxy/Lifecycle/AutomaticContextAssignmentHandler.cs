using Namotion.Proxy.Abstractions;

namespace Namotion.Proxy.Lifecycle;

internal class AutomaticContextAssignmentHandler : IProxyLifecycleHandler
{
    public void OnProxyAttached(ProxyLifecycleContext context)
    {
        if (context.ReferenceCount == 1)
        {
            context.Proxy.Context = context.Context;
        }
    }

    public void OnProxyDetached(ProxyLifecycleContext context)
    {
        if (context.ReferenceCount == 0)
        {
            context.Proxy.Context = null;
        }
    }
}

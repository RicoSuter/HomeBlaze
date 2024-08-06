using Namotion.Proxy.Abstractions;

namespace Namotion.Proxy.Lifecycle;

public class PropertyValueEqualityCheckHandler : IProxyWriteHandler
{
    public void WriteProperty(ProxyPropertyWriteContext context, Action<ProxyPropertyWriteContext> next)
    {
        if (!Equals(context.CurrentValue, context.NewValue))
        {
            next(context);
        }
    }
}

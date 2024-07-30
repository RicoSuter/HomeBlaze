using Namotion.Proxy.Abstractions;
using Namotion.Proxy.ChangeTracking;

namespace Namotion.Proxy;

public static class ProxyContextExtensions
{
    public static THandler GetHandler<THandler>(this IProxyContext context)
        where THandler : IProxyHandler
    {
        return context.GetHandlers<THandler>().Single();
    }

    public static IObservable<ProxyPropertyChanged> GetPropertyChangedObservable(this IProxyContext context)
    {
        return context.GetHandlers<PropertyChangedObservable>().Single();
    }
}

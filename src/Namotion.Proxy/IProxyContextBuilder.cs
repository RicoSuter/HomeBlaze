using Namotion.Proxy.Abstractions;

namespace Namotion.Proxy
{
    public interface IProxyContextBuilder
    {
        Lazy<THandler[]> GetLazyHandlers<THandler>()
            where THandler : IProxyHandler;

        ProxyContextBuilder AddHandler<T>(T handler)
            where T : IProxyHandler;

        ProxyContextBuilder TryAddSingleHandler<T>(T handler)
            where T : IProxyHandler;

        ProxyContext Build();
    }
}
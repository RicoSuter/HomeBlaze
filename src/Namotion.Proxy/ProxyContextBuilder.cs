using Namotion.Proxy.Abstractions;

namespace Namotion.Proxy;

public class ProxyContextBuilder : IProxyContextBuilder
{
    private readonly List<IProxyHandler> _handlers = new();

    public ProxyContextBuilder AddHandler<T>(T handler)
        where T : IProxyHandler
    {
        _handlers.Add(handler);
        return this;
    }

    public ProxyContextBuilder TryAddSingleHandler<T>(T handler)
        where T : IProxyHandler
    {
        if (_handlers.OfType<T>().Any() == false)
        {
            _handlers.Add(handler);
        }
        return this;
    }

    public ProxyContext Build()
    {
        return new ProxyContext(_handlers);
    }

    public Lazy<THandler[]> GetLazyHandlers<THandler>()
        where THandler : IProxyHandler
    {
        return new Lazy<THandler[]>(() => _handlers.OfType<THandler>().ToArray());
    }
}

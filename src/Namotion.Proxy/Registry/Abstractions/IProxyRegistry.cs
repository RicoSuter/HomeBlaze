using Namotion.Proxy.Abstractions;

namespace Namotion.Proxy.Registry.Abstractions;

public interface IProxyRegistry : IProxyHandler
{
    IReadOnlyDictionary<IProxy, RegisteredProxy> KnownProxies { get; }
}

using System.Collections.Concurrent;

namespace Namotion.Proxy;

public interface IProxy
{
    /// <summary>
    /// Gets the currently associated context.
    /// </summary>
    IProxyContext? Context { get; set; }

    /// <summary>
    /// Gets the additional data of this proxy.
    /// </summary>
    ConcurrentDictionary<string, object?> Data { get; }

    /// <summary>
    /// Gets the reflected properties (should be cached).
    /// </summary>
    IReadOnlyDictionary<string, ProxyPropertyInfo> Properties { get; }
}

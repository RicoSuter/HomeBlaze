using Namotion.Proxy.Abstractions;

namespace Namotion.Proxy;

public interface IProxyContext
{
    // TODO: Move to another interface
    IEnumerable<THandler> GetHandlers<THandler>()
        where THandler : IProxyHandler;

    object? GetProperty(IProxy proxy, string propertyName, Func<object?> readValue);

    void SetProperty(IProxy proxy, string propertyName, object? newValue, Func<object?> readValue, Action<object?> writeValue);
}

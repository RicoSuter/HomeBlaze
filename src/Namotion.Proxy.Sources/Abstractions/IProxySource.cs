namespace Namotion.Proxy.Sources.Abstractions;

public interface IProxySource
{
    string? TryGetSourcePath(ProxyPropertyReference property);

    Task<IDisposable?> InitializeAsync(IEnumerable<ProxyPropertyPathReference> properties, Action<ProxyPropertyPathReference> propertyUpdateAction, CancellationToken cancellationToken);

    // TODO: Should return dict<path, obj?>?
    Task<IEnumerable<ProxyPropertyPathReference>> ReadAsync(IEnumerable<ProxyPropertyPathReference> properties, CancellationToken cancellationToken);

    Task WriteAsync(IEnumerable<ProxyPropertyPathReference> propertyChanges, CancellationToken cancellationToken);
}

public record struct ProxyPropertyPathReference(ProxyPropertyReference Property, string Path, object? Value);

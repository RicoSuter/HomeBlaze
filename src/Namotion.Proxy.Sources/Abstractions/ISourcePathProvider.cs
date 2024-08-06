namespace Namotion.Proxy.Sources.Abstractions;

public interface ISourcePathProvider
{
    string? TryGetSourcePropertyName(ProxyPropertyReference property);

    string? TryGetSourcePath(ProxyPropertyReference property);
}

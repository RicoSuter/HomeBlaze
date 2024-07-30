namespace Namotion.Proxy.Registry.Abstractions;

public readonly record struct ProxyPropertyChild
{
    public IProxy Proxy { get; init; }

    public object? Index { get; init; }
}
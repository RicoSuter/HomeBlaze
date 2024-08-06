namespace Namotion.Proxy.Registry.Abstractions;

public interface IProxyPropertyInitializer
{
    void InitializeProperty(RegisteredProxyProperty property, object? index, IProxyContext context);
}
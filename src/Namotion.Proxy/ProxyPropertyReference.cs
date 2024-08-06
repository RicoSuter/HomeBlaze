namespace Namotion.Proxy;

public record struct ProxyPropertyReference(IProxy Proxy, string Name)
{
    // TODO: Rename to Info?
    public readonly ProxyPropertyInfo Metadata => Proxy.Properties[Name];

    public readonly void SetPropertyData(string key, object? value)
    {
        Proxy.Data[$"{key}:{Name}"] = value;
    }

    public readonly bool TryGetPropertyData(string key, out object? value)
    {
        return Proxy.Data.TryGetValue($"{key}:{Name}", out value);
    }

    public readonly object? GetPropertyData(string key)
    {
        return Proxy.Data[$"{key}:{Name}"];
    }

    public readonly T GetOrAddPropertyData<T>(string key, Func<T> valueFactory)
    {
        return (T)Proxy.Data.GetOrAdd($"{key}:{Name}", _ => valueFactory())!;
    }
}

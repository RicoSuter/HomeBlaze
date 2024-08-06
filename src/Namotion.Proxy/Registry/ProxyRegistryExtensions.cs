using Namotion.Proxy.Attributes;
using Namotion.Proxy.Registry.Abstractions;

namespace Namotion.Proxy.Registry;

public static class ProxyRegistryExtensions
{
    public static IEnumerable<RegisteredProxyProperty> GetProperties(this IProxyRegistry registry)
    {
        foreach (var pair in registry.KnownProxies)
        {
            foreach (var property in pair.Value.Properties.Values)
            {
                yield return property;
            }
        }
    }

    public static RegisteredProxyProperty? TryGetProperty(this IReadOnlyDictionary<IProxy, RegisteredProxy> properties, ProxyPropertyReference property)
    {
        if (properties.TryGetValue(property.Proxy, out var metadata))
        {
            if (metadata.Properties.TryGetValue(property.Name, out var result))
            {
                return result;
            }
        }

        return null;
    }

    public static RegisteredProxyProperty? TryGetPropertyAttribute(this ProxyPropertyReference property, string attributeName)
    {
        // TODO: Also support non-registry scenario

        var registry = property.Proxy.Context?.GetHandler<IProxyRegistry>() 
            ?? throw new InvalidOperationException($"The {nameof(IProxyRegistry)} is missing.");
        
        var attribute = registry.KnownProxies[property.Proxy].Properties
            .SingleOrDefault(p => p.Value.Attributes
                .OfType<PropertyAttributeAttribute>()
                .Any(a => a.PropertyName == property.Name && a.AttributeName == attributeName));

        return attribute.Value;
    }

    public static IEnumerable<KeyValuePair<string, RegisteredProxyProperty>> GetPropertyAttributes(this ProxyPropertyReference property)
    {
        // TODO: Also support non-registry scenario

        var registry = property.Proxy.Context?.GetHandler<IProxyRegistry>()
            ?? throw new InvalidOperationException($"The {nameof(IProxyRegistry)} is missing.");

        return registry.KnownProxies[property.Proxy].Properties
            .Where(p => p.Value.Attributes
                .OfType<PropertyAttributeAttribute>()
                .Any(a => a.PropertyName == property.Name));
    }
}

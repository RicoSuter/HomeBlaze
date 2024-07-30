using Namotion.Proxy.Abstractions;
using Namotion.Proxy.Attributes;
using Namotion.Proxy.Lifecycle;
using Namotion.Proxy.Registry.Abstractions;

using System.Collections;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace Namotion.Proxy;

public static class ProxyExtensions
{
    /// <summary>
    /// Will attach the proxy and its children to the context and 
    /// detach only the proxy itself from the previous context.
    /// </summary>
    /// <param name="proxy">The proxy.</param>
    /// <param name="context">The context.</param>
    public static void SetContext(this IProxy proxy, IProxyContext? context)
    {
        var currentContext = proxy.Context;
        if (currentContext != context)
        {
            if (currentContext is not null)
            {
                var registryContext = new ProxyLifecycleContext(default, null, proxy, 0, currentContext);
                foreach (var handler in currentContext.GetHandlers<IProxyLifecycleHandler>())
                {
                    handler.OnProxyDetached(registryContext);
                }
            }

            proxy.Context = context;

            if (context is not null)
            {
                var registryContext = new ProxyLifecycleContext(default, null, proxy, 1, context);
                foreach (var handler in context.GetHandlers<IProxyLifecycleHandler>())
                {
                    handler.OnProxyAttached(registryContext);
                }
            }
        }
    }

    public static void SetData(this IProxy proxy, string key, object? value)
    {
        proxy.Data[key] = value;
    }

    public static bool TryGetData(this IProxy proxy, string key, out object? value)
    {
        return proxy.Data.TryGetValue(key, out value);
    }

    public static string GetJsonPath(this ProxyPropertyReference property)
    {
        var registry = property.Proxy.Context?.GetHandler<IProxyRegistry>()
          ?? throw new InvalidOperationException($"The {nameof(IProxyRegistry)} is missing.");

        if (registry is not null)
        {
            // TODO: avoid endless recursion
            string? path = null;
            var parent = new ProxyParent(property, null);
            do
            {
                var attribute = registry
                    .KnownProxies[parent.Property.Proxy]
                    .Properties[parent.Property.Name]
                    .Attributes
                    .OfType<PropertyAttributeAttribute>()
                    .FirstOrDefault();

                if (attribute is not null)
                {
                    return GetJsonPath(new ProxyPropertyReference(
                        parent.Property.Proxy,
                        attribute.PropertyName)) +
                        "@" + attribute.AttributeName;
                }

                path = JsonNamingPolicy.CamelCase.ConvertName(parent.Property.Name) +
                    (parent.Index is not null ? $"[{parent.Index}]" : string.Empty) +
                    (path is not null ? $".{path}" : string.Empty);

                parent = parent.Property.Proxy.GetParents().FirstOrDefault();
            }
            while (parent.Property.Proxy is not null);

            return path.Trim('.');
        }
        else
        {
            // TODO: avoid endless recursion
            string? path = null;
            var parent = new ProxyParent(property, null);
            do
            {
                var attribute = parent.Property.Proxy
                    .Properties[parent.Property.Name]
                    .Attributes
                    .OfType<PropertyAttributeAttribute>()
                    .FirstOrDefault();

                if (attribute is not null)
                {
                    return GetJsonPath(new ProxyPropertyReference(
                        parent.Property.Proxy,
                        attribute.PropertyName)) +
                        "@" + attribute.AttributeName;
                }

                path = JsonNamingPolicy.CamelCase.ConvertName(parent.Property.Name) +
                    (parent.Index is not null ? $"[{parent.Index}]" : string.Empty) +
                    (path is not null ? $".{path}" : string.Empty);

                parent = parent.Property.Proxy.GetParents().FirstOrDefault();
            }
            while (parent.Property.Proxy is not null);

            return path.Trim('.');
        }
    }

    public static JsonObject ToJsonObject(this IProxy proxy)
    {
        var obj = new JsonObject();
        foreach (var property in proxy
            .Properties
            .Where(p => p.Value.GetValue is not null))
        {
            var propertyName = GetJsonPropertyName(proxy, property.Value);
            var value = property.Value.GetValue?.Invoke(proxy);
            if (value is IProxy childProxy)
            {
                obj[propertyName] = childProxy.ToJsonObject();
            }
            else if (value is ICollection collection && collection.OfType<IProxy>().Any())
            {
                var children = new JsonArray();
                foreach (var arrayProxyItem in collection.OfType<IProxy>())
                {
                    children.Add(arrayProxyItem.ToJsonObject());
                }
                obj[propertyName] = children;
            }
            else
            {
                obj[propertyName] = JsonValue.Create(value);
            }
        }

        var registry = proxy.Context?.GetHandler<IProxyRegistry>()
            ?? throw new InvalidOperationException($"The {nameof(IProxyRegistry)} is missing.");

        if (registry?.KnownProxies.TryGetValue(proxy, out var metadata) == true)
        {
            foreach (var property in metadata
                .Properties
                .Where(p => p.Value.HasGetter && 
                            proxy.Properties.ContainsKey(p.Key) == false))
            {
                var propertyName = property.GetJsonPropertyName();
                var value = property.Value.GetValue();
                if (value is IProxy childProxy)
                {
                    obj[propertyName] = childProxy.ToJsonObject();
                }
                else if (value is ICollection collection && collection.OfType<IProxy>().Any())
                {
                    var children = new JsonArray();
                    foreach (var arrayProxyItem in collection.OfType<IProxy>())
                    {
                        children.Add(arrayProxyItem.ToJsonObject());
                    }
                    obj[propertyName] = children;
                }
                else
                {
                    obj[propertyName] = JsonValue.Create(value);
                }
            }
        }

        return obj;
    }

    private static string GetJsonPropertyName(IProxy proxy, ProxyPropertyInfo property)
    {
        var attribute = property
            .Attributes
            .OfType<PropertyAttributeAttribute>()
            .FirstOrDefault();

        if (attribute is not null)
        {
            var propertyName = GetJsonPropertyName(proxy, proxy.Properties[attribute.PropertyName]);
            return $"{propertyName}@{attribute.AttributeName}";
        }

        return JsonNamingPolicy.CamelCase.ConvertName(property.Name);
    }

    public static string GetJsonPropertyName(this KeyValuePair<string, RegisteredProxyProperty> property)
    {
        var attribute = property
            .Value
            .Attributes
            .OfType<PropertyAttributeAttribute>()
            .FirstOrDefault();

        if (attribute is not null)
        {
            return property.Value
                .Parent.Properties
                .Single(p => p.Key == attribute.PropertyName) // TODO: Improve performance??
                .GetJsonPropertyName() + "@" + attribute.AttributeName;
        }

        return JsonNamingPolicy.CamelCase.ConvertName(property.Key);
    }

    public static (IProxy?, ProxyPropertyInfo) FindPropertyFromJsonPath(this IProxy proxy, string path)
    {
        return proxy.FindPropertyFromJsonPath(path.Split('.'));
    }

    private static (IProxy?, ProxyPropertyInfo) FindPropertyFromJsonPath(this IProxy proxy, IEnumerable<string> segments)
    {
        var nextSegment = segments.First();
        nextSegment = ConvertToUpperCamelCase(nextSegment);

        segments = segments.Skip(1);
        if (segments.Any())
        {
            if (nextSegment.Contains('['))
            {
                var arraySegments = nextSegment.Split('[', ']');
                var index = int.Parse(arraySegments[1]);
                nextSegment = arraySegments[0];

                var collection = proxy.Properties[nextSegment].GetValue?.Invoke(proxy) as ICollection;
                var child = collection?.OfType<IProxy>().ElementAt(index);
                return child is not null ? FindPropertyFromJsonPath(child, segments) : (null, default);
            }
            else
            {
                var child = proxy.Properties[nextSegment].GetValue?.Invoke(proxy) as IProxy;
                return child is not null ? FindPropertyFromJsonPath(child, segments) : (null, default);
            }
        }
        else
        {
            return (proxy, proxy.Properties[nextSegment]);
        }
    }

    private static string ConvertToUpperCamelCase(string nextSegment)
    {
        if (string.IsNullOrEmpty(nextSegment))
        {
            return nextSegment;
        }

        var characters = nextSegment.ToCharArray();
        characters[0] = char.ToUpperInvariant(characters[0]);
        return new string(characters);
    }
}

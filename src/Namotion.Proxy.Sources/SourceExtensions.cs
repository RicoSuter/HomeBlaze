using Namotion.Proxy.Abstractions;
using Namotion.Proxy.Sources.Abstractions;

namespace Namotion.Proxy.Sources;

public static class SourceExtensions
{
    private const string SourcePropertyNameKey = "Namotion.SourcePropertyName:";
    private const string SourcePathKey = "Namotion.SourcePath:";
    private const string SourcePathPrefixKey = "Namotion.SourcePathPrefix:";

    private const string IsChangingFromSourceKey = "Namotion.IsChangingFromSource";

    public static void SetValueFromSource(this ProxyPropertyReference property, IProxySource source, object? valueFromSource)
    {
        var contexts = property.GetOrAddPropertyData(IsChangingFromSourceKey, () => new HashSet<IProxySource>())!;
        lock (contexts)
        {
            contexts.Add(source);
        }

        try
        {
            var newValue = valueFromSource;

            var currentValue = property.Metadata.GetValue?.Invoke(property.Proxy);
            if (!Equals(currentValue, newValue))
            {
                property.Metadata.SetValue?.Invoke(property.Proxy, newValue);
            }
        }
        finally
        {
            lock (contexts)
            {
                contexts.Remove(source);
            }
        }
    }

    public static bool IsChangingFromSource(this ProxyPropertyChanged change, IProxySource source)
    {
        var contexts = change.Property.GetOrAddPropertyData(IsChangingFromSourceKey, () => new HashSet<IProxySource>())!;
        lock (contexts)
        {
            return contexts.Contains(source);
        }
    }

    public static string? TryGetAttributeBasedSourcePropertyName(this ProxyPropertyReference property, string sourceName)
    {
        return property.TryGetPropertyData($"{SourcePropertyNameKey}{sourceName}", out var value) ? value as string : null;
    }

    public static string? TryGetAttributeBasedSourcePath(this ProxyPropertyReference property, string sourceName, IProxyContext context)
    {
        return property.TryGetPropertyData($"{SourcePathKey}{sourceName}", out var value) ? value as string : null;
    }

    public static string? TryGetAttributeBasedSourcePathPrefix(this ProxyPropertyReference property, string sourceName)
    {
        return property.TryGetPropertyData($"{SourcePathPrefixKey}{sourceName}", out var value) ? value as string : null;
    }

    public static void SetAttributeBasedSourceProperty(this ProxyPropertyReference property, string sourceName, string sourceProperty)
    {
        property.SetPropertyData($"{SourcePropertyNameKey}{sourceName}", sourceProperty);
    }

    public static void SetAttributeBasedSourcePathPrefix(this ProxyPropertyReference property, string sourceName, string sourcePath)
    {
        property.SetPropertyData($"{SourcePathPrefixKey}{sourceName}", sourcePath);
    }

    public static void SetAttributeBasedSourcePath(this ProxyPropertyReference property, string sourceName, string sourcePath)
    {
        property.SetPropertyData($"{SourcePathKey}{sourceName}", sourcePath);
    }
}

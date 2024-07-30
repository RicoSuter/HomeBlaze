namespace Namotion.Proxy.ChangeTracking;

public static class DerivedPropertyChangeDetectionHandlerExtensions
{
    private const string UsedByPropertiesKey = "Namotion.Proxy.UsedByProperties";
    private const string RequiredPropertiesKey = "Namotion.Proxy.RequiredProperties";
    private const string LastKnownValueKey = "Namotion.Proxy.LastKnownValue";

    public static HashSet<ProxyPropertyReference> GetUsedByProperties(this ProxyPropertyReference property)
    {
        return property.GetOrAddPropertyData(UsedByPropertiesKey, () => new HashSet<ProxyPropertyReference>());
    }

    public static HashSet<ProxyPropertyReference> GetRequiredProperties(this ProxyPropertyReference property)
    {
        return property.GetOrAddPropertyData(RequiredPropertiesKey, () => new HashSet<ProxyPropertyReference>());
    }

    internal static void SetRequiredProperties(this ProxyPropertyReference property, HashSet<ProxyPropertyReference> requiredProperties)
    {
        property.SetPropertyData(RequiredPropertiesKey, requiredProperties);
    }

    internal static object? GetLastKnownValue(this ProxyPropertyReference property)
    {
        return property.GetOrAddPropertyData(LastKnownValueKey, () => (object?)null);
    }

    internal static void SetLastKnownValue(this ProxyPropertyReference property, object? value)
    {
        property.SetPropertyData(LastKnownValueKey, value);
    }
}

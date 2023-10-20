using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Namotion.Trackable.Model;

public record TrackablePropertyChange
{
    public TrackableProperty Property { get; }

    public object? Value { get; }

    [JsonExtensionData]
    public IDictionary<string, object?> PropertyDataSnapshot { get; }

    public TrackablePropertyChange(TrackableProperty property, IDictionary<string, object?> propertyDataSnapshot, object? value)
    {
        Property = property;
        PropertyDataSnapshot = propertyDataSnapshot;
        Value = value;
    }
}

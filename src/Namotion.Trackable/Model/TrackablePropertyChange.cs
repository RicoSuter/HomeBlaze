using System.Collections.Generic;

namespace Namotion.Trackable.Model;

public record TrackablePropertyChange
{
    public TrackedProperty Property { get; }

    public object? Value { get; }

    public IDictionary<string, object?> PropertyDataSnapshot { get; }

    public TrackablePropertyChange(TrackedProperty property, IDictionary<string, object?> propertyDataSnapshot, object? value)
    {
        Property = property;
        PropertyDataSnapshot = propertyDataSnapshot;
        Value = value;
    }
}

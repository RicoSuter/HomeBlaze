using System.Collections.Generic;

namespace Namotion.Trackable.Model;

public record TrackedPropertyChange
{
    public TrackedProperty Property { get; }

    public object? Value { get; }

    public IDictionary<string, object?> PropertyDataSnapshot { get; }

    public TrackedPropertyChange(TrackedProperty property, IDictionary<string, object?> propertyDataSnapshot, object? value)
    {
        Property = property;
        PropertyDataSnapshot = propertyDataSnapshot;
        Value = value;
    }
}

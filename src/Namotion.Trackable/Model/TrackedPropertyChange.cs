using System.Collections.Generic;

namespace Namotion.Trackable.Model;

public record TrackedPropertyChange
{
    public TrackedProperty Property { get; }

    public object? Value { get; }

    public IReadOnlyDictionary<string, object?> PropertyDataSnapshot { get; }

    public TrackedPropertyChange(TrackedProperty property, IReadOnlyDictionary<string, object?> propertyDataSnapshot, object? value)
    {
        Property = property;
        PropertyDataSnapshot = propertyDataSnapshot;
        Value = value;
    }
}

using System;

namespace Namotion.Trackable.Model;

public class StaticTrackedProperty : TrackedProperty
{
    public StaticTrackedProperty(string name, Tracker parent, object? value)
        : base(name, parent)
    {
        Value = value;
    }

    public override bool IsReadable => true;

    public override bool IsWriteable => false;

    public override bool IsDerived => false;

    public override Type PropertyType => Value?.GetType() ?? typeof(object);

    public static TrackedProperty CreateAttribute(string propertyName, string attributeName, Tracker parent, object? value)
    {
        var property = new StaticTrackedProperty($"{propertyName}.{attributeName}", parent, value);
        property.ConvertToAttribute(attributeName, propertyName);
        return property;
    }

    public override object? GetValue()
    {
        return Value;
    }

    public override void SetValue(object? value)
    {
        throw new InvalidOperationException("Cannot set value of static property.");
    }
}

using System;
using System.Reflection;

namespace Namotion.Trackable.Model;

public class ReflectionTrackedProperty : TrackedProperty
{
    private readonly PropertyInfo _property;

    public ReflectionTrackedProperty(PropertyInfo property, Tracker parent)
        : this(property.Name, property, parent)
    {
    }

    public ReflectionTrackedProperty(string name, PropertyInfo property, Tracker parent) 
        : base(name, parent)
    {
        _property = property;

        IsReadable = property.GetMethod != null;
        IsWriteable = property.SetMethod != null;
        IsDerived = property.SetMethod == null;
        PropertyType = property.PropertyType;
    }

    public override bool IsReadable { get; }

    public override bool IsWriteable { get; }

    public override bool IsDerived { get; }

    public override Type PropertyType { get; }

    public override object? GetValue()
    {
        return _property.GetValue(Parent.Object);
    }

    public override void SetValue(object? value)
    {
        _property.SetValue(Parent.Object, value);
    }
}

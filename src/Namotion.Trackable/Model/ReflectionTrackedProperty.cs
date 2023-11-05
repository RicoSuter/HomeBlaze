using System;
using System.Reflection;

namespace Namotion.Trackable.Model;

public class ReflectionTrackedProperty : TrackedProperty
{
    private MethodInfo? _getMethod;
    private MethodInfo? _setMethod;

    private readonly PropertyInfo _property;

    public ReflectionTrackedProperty(PropertyInfo property, Tracker parent)
        : this(property.Name, property, parent)
    {
    }

    public ReflectionTrackedProperty(string name, PropertyInfo property, Tracker parent) 
        : base(name, parent)
    {
        _property = property;

        _getMethod = property.GetMethod;
        _setMethod = property.SetMethod;
    }

    public override bool IsReadable => _getMethod != null;

    public override bool IsWriteable => _setMethod != null;

    public override bool IsDerived => _setMethod == null;

    public override Type PropertyType => _property.PropertyType;

    public override object? GetValue()
    {
        return _property.GetValue(Parent.Object);
    }

    public override void SetValue(object? value)
    {
        _property.SetValue(Parent.Object, value);
    }
}

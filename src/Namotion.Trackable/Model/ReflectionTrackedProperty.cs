using System;

namespace Namotion.Trackable.Model;

public class ReflectionTrackedProperty : TrackedProperty
{
    private readonly PropertyReflectionMetadata _propertyReflectionMetadata;

    public ReflectionTrackedProperty(PropertyReflectionMetadata property, Tracker parent)
        : this(property.Name, property, parent)
    {
    }

    public ReflectionTrackedProperty(string name, PropertyReflectionMetadata propertyReflectionMetadata, Tracker parent) 
        : base(name, parent)
    {
        _propertyReflectionMetadata = propertyReflectionMetadata;

        IsReadable = propertyReflectionMetadata.IsReadable;
        IsWriteable = propertyReflectionMetadata.IsWriteable;
        IsDerived = propertyReflectionMetadata.IsDerived;
        PropertyType = propertyReflectionMetadata.PropertyType;
    }

    public override bool IsReadable { get; }

    public override bool IsWriteable { get; }

    public override bool IsDerived { get; }

    public override Type PropertyType { get; }

    public override object? GetValue()
    {
        return _propertyReflectionMetadata.GetValue(Parent.Object);
    }

    public override void SetValue(object? value)
    {
        _propertyReflectionMetadata.SetValue(Parent.Object, value);
    }
}

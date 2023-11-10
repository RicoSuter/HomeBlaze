using System;

namespace Namotion.Trackable.Model;

public class ProxyTrackedProperty : TrackedProperty
{
    private readonly PropertyReflectionMetadata _propertyReflectionMetadata;
    private readonly ProxyTracker _parent;

    public ProxyTrackedProperty(PropertyReflectionMetadata property, ProxyTracker parent)
        : this(property.Name, property, parent)
    {
    }

    public ProxyTrackedProperty(string name, PropertyReflectionMetadata propertyReflectionMetadata, ProxyTracker parent) 
        : base(name, parent)
    {
        _parent = parent;
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
        return _propertyReflectionMetadata.GetValue(_parent.Object);
    }

    public override void SetValue(object? value)
    {
        _propertyReflectionMetadata.SetValue(_parent.Object, value);
    }
}

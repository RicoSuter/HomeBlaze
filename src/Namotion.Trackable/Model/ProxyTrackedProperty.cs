using System;

namespace Namotion.Trackable.Model;

public class ProxyTrackedProperty : TrackedProperty
{
    private readonly PropertyReflectionMetadata _propertyReflectionMetadata;
    private readonly ProxyTracker _parent;

    public ProxyTrackedProperty(PropertyReflectionMetadata property,
        ProxyTracker parent, IObserver<TrackedPropertyChange> observer)
        : this(property.Name, property, parent, observer)
    {
    }

    public ProxyTrackedProperty(string name, PropertyReflectionMetadata propertyReflectionMetadata,
        ProxyTracker parent, IObserver<TrackedPropertyChange> observer)
        : base(name, parent, observer)
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

    public override object? Value
    {
        get => _propertyReflectionMetadata.GetValue(_parent.Object);
        set => _propertyReflectionMetadata.SetValue(_parent.Object, value);
    }
}

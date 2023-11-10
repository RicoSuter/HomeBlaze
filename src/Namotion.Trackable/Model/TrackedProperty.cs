using System;

namespace Namotion.Trackable.Model;

public class TrackedProperty<TProperty> : TrackedProperty
{
    public TrackedProperty(
        string name, TProperty? value, Tracker parent, IObserver<TrackedPropertyChange> observer)
        : base(name, parent, observer)
    {
        base.LastValue = value;
    }

    public override bool IsReadable => true;

    public override bool IsWriteable => true;

    public override bool IsDerived => false;

    public new TProperty? LastValue => (TProperty?)base.LastValue;

    public override Type PropertyType => LastValue?.GetType() ?? typeof(object);

    public static TrackedProperty CreateAttribute(TrackedProperty attributedProperty, string attributeName, TProperty? value, IObserver<TrackedPropertyChange> observer)
    {
        var property = new TrackedProperty<TProperty>($"{attributedProperty.Name}.{attributeName}", value, attributedProperty.Parent, observer);
        property.ConvertToAttribute(attributeName, attributedProperty.Name);
        return property;
    }
}

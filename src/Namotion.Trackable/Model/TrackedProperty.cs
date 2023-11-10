using System;

namespace Namotion.Trackable.Model;

public class TrackedProperty<TProperty> : TrackedProperty
{
    public TrackedProperty(string name, TProperty? value, Tracker parent, IObserver<TrackedPropertyChange> observer)
        : base(name, parent, observer)
    {
        base.LastKnownValue = value;
    }

    public override bool IsReadable => true;

    public override bool IsWriteable => true;

    public override bool IsDerived => false;

    /// <summary>
    /// <inheritdoc cref="TrackedProperty.LastKnownValue"/>/>
    /// </summary>
    public new TProperty? LastKnownValue => (TProperty?)base.LastKnownValue;

    public override Type PropertyType => typeof(TProperty);

    public static TrackedProperty CreateAttribute(TrackedProperty attributedProperty, string attributeName, TProperty? value, IObserver<TrackedPropertyChange> observer)
    {
        var property = new TrackedProperty<TProperty>($"{attributedProperty.Name}.{attributeName}", value, attributedProperty.Parent, observer);
        property.ConvertToAttribute(attributeName, attributedProperty.Name);
        return property;
    }
}

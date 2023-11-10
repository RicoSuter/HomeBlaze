using System;

namespace Namotion.Trackable.Model;

public class DerivedTrackedProperty<TProperty> : TrackedProperty
{
    private readonly Func<TProperty?>? _getValue;
    private readonly Action<TProperty?>? _setValue;

    public DerivedTrackedProperty(string name, Func<TProperty?>? getValue, Action<TProperty?>? setValue, Tracker parent, IObserver<TrackedPropertyChange> observer)
        : base(name, parent, observer)
    {
        _getValue = getValue;
        _setValue = setValue;

        base.LastKnownValue = getValue != null ? getValue() : null;
    }

    public override bool IsReadable => _getValue != null;

    public override bool IsWriteable => _setValue != null;

    public override bool IsDerived => true;

    public override Type PropertyType => LastKnownValue?.GetType() ?? typeof(object);

    public override object? Value
    {
        get
        {
            OnBeforeRead();
            try
            {
                object? value = _getValue != null ? _getValue() : null;
                LastKnownValue = value;
                return value;
            }
            finally
            {
                OnAfterRead();
            }
        }

        set => _setValue?.Invoke((TProperty?)value);
    }

    public static TrackedProperty CreateAttribute(TrackedProperty attributedProperty, string attributeName, TProperty? value, IObserver<TrackedPropertyChange> observer)
    {
        var property = new TrackedProperty<TProperty>($"{attributedProperty.Name}.{attributeName}", value, attributedProperty.Parent, observer);
        property.ConvertToAttribute(attributeName, attributedProperty.Name);
        return property;
    }
}

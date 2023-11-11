using System;

namespace Namotion.Trackable.Model;

public class DerivedTrackedProperty<TProperty> : TrackedProperty
{
    private readonly Func<TProperty>? _getValue;
    private readonly Action<TProperty, Action<TProperty>>? _setValue;

    public DerivedTrackedProperty(string name, Func<TProperty>? getValue, Action<TProperty, Action<TProperty>>? setValue, Tracker parent, IObserver<TrackedPropertyChange> observer)
        : base(name, parent, observer)
    {
        _getValue = getValue;
        _setValue = setValue;

        base.LastKnownValue = GetValue();
    }

    public override bool IsReadable => _getValue != null;

    public override bool IsWriteable => _setValue != null;

    public override bool IsDerived => true;

    /// <summary>
    /// <inheritdoc cref="TrackedProperty.LastKnownValue"/>/>
    /// </summary>
    public new TProperty? LastKnownValue => (TProperty?)base.LastKnownValue;

    public override Type PropertyType => LastKnownValue?.GetType() ?? typeof(object);

    public TProperty Value
    {
        get
        {
            var value = GetValue();
            return value != null ? (TProperty)value : default!;
        }

        set => SetValue(value);
    }

    public override object? GetValue()
    {
        _ = _getValue ?? throw new NotImplementedException();

        OnBeforeRead();
        try
        {
            object? value = _getValue != null ? _getValue() : null;
            base.LastKnownValue = value;
            return value;
        }
        finally
        {
            OnAfterRead();
        }
    }

    public override void SetValue(object? value)
    {
        _ = _setValue ?? throw new NotImplementedException();

        _setValue?.Invoke(value != null ? (TProperty)value : default!, (v) => base.SetValue(v));
    }

    public static TrackedProperty CreateAttribute(TrackedProperty attributedProperty, string attributeName, 
        Func<TProperty>? getValue, Action<TProperty, Action<TProperty>>? setValue, IObserver<TrackedPropertyChange> observer)
    {
        var property = new DerivedTrackedProperty<TProperty>(
            $"{attributedProperty.Name}.{attributeName}", 
            getValue, setValue, attributedProperty.Parent, observer);
      
        property.ConvertToAttribute(attributeName, attributedProperty.Name);
        return property;
    }
}

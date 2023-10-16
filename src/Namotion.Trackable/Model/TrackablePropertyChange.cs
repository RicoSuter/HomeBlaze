namespace Namotion.Trackable.Model;

public record TrackablePropertyChange
{
    public TrackableProperty Property { get; }

    public object? Value { get; }

    public bool IsUpdatedFromSource { get; }

    public TrackablePropertyChange(TrackableProperty property, object? value, bool isUpdatedFromSource)
    {
        Property = property;
        Value = value;
        IsUpdatedFromSource = isUpdatedFromSource;
    }
}

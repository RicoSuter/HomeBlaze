using Namotion.Trackable.Model;

namespace Namotion.Trackable;

public interface IFromSourceConverter : IPropertyProcessor
{
    object? ConvertFromSource(TrackedProperty property, object? value);
}

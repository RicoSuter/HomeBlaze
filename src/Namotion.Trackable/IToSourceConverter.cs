using Namotion.Trackable.Model;

namespace Namotion.Trackable;

public interface IToSourceConverter : IPropertyProcessor
{
    object? ConvertToSource(TrackedProperty property, object? value);
}

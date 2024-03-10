using Namotion.Trackable.Model;

namespace Namotion.Trackable.Attributes;

public interface ITrackablePropertyInitializer : IPropertyProcessor
{
    void InitializeProperty(TrackedProperty property, object? parentCollectionKey, ITrackableContext context);
}
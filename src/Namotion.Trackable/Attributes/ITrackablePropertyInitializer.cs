using Namotion.Trackable.Model;

namespace Namotion.Trackable.Attributes;

public interface ITrackablePropertyInitializer
{
    void InitializeProperty(TrackedProperty property, object? parentCollectionKey, ITrackableContext context);
}
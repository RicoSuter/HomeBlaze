using Namotion.Trackable.Model;

namespace Namotion.Trackable.Attributes;

public interface ITrackablePropertyInitializer
{
    void InitializeProperty(TrackedProperty property, Tracker parent, object? parentCollectionKey, ITrackableContext context);
}
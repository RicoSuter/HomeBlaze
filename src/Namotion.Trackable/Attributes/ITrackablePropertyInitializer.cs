using Namotion.Trackable.Model;

namespace Namotion.Trackable.Attributes;

public interface ITrackablePropertyInitializer
{
    void InitializeProperty(ITrackableContext context, TrackedProperty property, Tracker parent, object? parentCollectionKey);
}
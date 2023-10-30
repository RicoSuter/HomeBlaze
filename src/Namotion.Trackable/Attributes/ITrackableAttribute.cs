using Namotion.Trackable.Model;

namespace Namotion.Trackable.Attributes;

public interface ITrackableAttribute
{
    void ProcessProperty(TrackedProperty property, Tracker parent, object? parentCollectionKey);
}
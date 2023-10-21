using Namotion.Trackable.Model;

namespace Namotion.Trackable.Attributes;

public interface ITrackableAttribute
{
    void ProcessCreatedTrackedProperty(TrackedProperty property, Tracker parent, int? parentCollectionIndex);
}
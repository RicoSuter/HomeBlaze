using Namotion.Trackable.Model;

namespace Namotion.Trackable.Attributes;

public interface ITrackableAttribute
{
    void OnTrackedPropertyCreated(TrackedProperty property, Tracker parent, int? parentCollectionIndex);
}
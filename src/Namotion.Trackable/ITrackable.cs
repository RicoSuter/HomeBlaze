namespace Namotion.Trackable;

public interface ITrackable
{
    void AddTrackableContext(ITrackableContext thingContext) { }

    void RemoveTrackableContext(ITrackableContext thingContext) { }
}

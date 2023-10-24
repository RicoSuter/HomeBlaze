namespace Namotion.Trackable;

public interface ITrackable
{
    void AddTrackableContext(ITrackableContext trackableContext) { }

    void RemoveTrackableContext(ITrackableContext trackableContext) { }
}

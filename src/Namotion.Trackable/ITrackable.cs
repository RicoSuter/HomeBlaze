namespace Namotion.Trackable;

public interface ITrackable
{
    void AddThingContext(ITrackableContext thingContext) { }

    void RemoveThingContext(ITrackableContext thingContext) { }
}

namespace Namotion.Trackable;

public interface ITrackable
{
    TrackableInterceptor Interceptor { get; }

    void AddTrackableContext(ITrackableContext trackableContext) { }

    void RemoveTrackableContext(ITrackableContext trackableContext) { }
}

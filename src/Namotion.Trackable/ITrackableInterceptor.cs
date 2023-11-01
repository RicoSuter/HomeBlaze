using Namotion.Trackable.Model;

namespace Namotion.Trackable;

public interface ITrackableInterceptor
{
    void OnBeforePropertyRead(TrackedProperty property, ITrackableContext trackableContext) { }

    void OnAfterPropertyRead(TrackedProperty property, object? newValue, ITrackableContext trackableContext) { }

    void OnBeforePropertyWrite(TrackedProperty property, object? newValue, object? previousValue, ITrackableContext trackableContext) { }

    void OnAfterPropertyWrite(TrackedProperty property, object? newValue, object? previousValue, ITrackableContext trackableContext) { }
}

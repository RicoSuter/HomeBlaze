using Namotion.Trackable.Model;

namespace Namotion.Trackable;

public interface ITrackableInterceptor
{
    void OnBeforePropertyRead(TrackedProperty getProperty, ITrackableContext trackableContext) { }

    void OnAfterPropertyRead(TrackedProperty getProperty, object? newValue, ITrackableContext trackableContext) { }

    void OnBeforePropertyWrite(TrackedProperty setProperty, object? newValue, object? previousValue, ITrackableContext trackableContext) { }

    void OnAfterPropertyWrite(TrackedProperty setProperty, object? newValue, object? previousValue, ITrackableContext trackableContext) { }
}

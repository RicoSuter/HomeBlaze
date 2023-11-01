using Castle.DynamicProxy;
using Namotion.Trackable.Model;

namespace Namotion.Trackable;

public interface ITrackableInterceptor
{
    void OnBeforeReadProperty(TrackedProperty property, ITrackableContext trackableContext) { }

    void OnAfterReadProperty(TrackedProperty property, object? newValue, ITrackableContext trackableContext) { }

    void OnBeforeWriteProperty(TrackedProperty property, object? newValue, object? previousValue, ITrackableContext trackableContext) { }

    void OnAfterWriteProperty(TrackedProperty property, object? newValue, object? previousValue, ITrackableContext trackableContext) { }
}

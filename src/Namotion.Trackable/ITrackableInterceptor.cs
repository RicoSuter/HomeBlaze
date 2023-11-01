using Castle.DynamicProxy;
using Namotion.Trackable.Model;

namespace Namotion.Trackable;

public interface ITrackableInterceptor
{
    bool OnReadProperty(IInvocation invocation) { return true; }

    void OnBeforeReadProperty(TrackedProperty property, ITrackableContext trackableContext) { }

    void OnAfterReadProperty(TrackedProperty property, object? newValue, ITrackableContext trackableContext) { }

    bool OnWriteProperty(IInvocation invocation) { return true; }

    void OnBeforeWriteProperty(TrackedProperty property, object? newValue, object? previousValue, ITrackableContext trackableContext) { }

    void OnAfterWriteProperty(TrackedProperty property, object? newValue, object? previousValue, ITrackableContext trackableContext) { }
}

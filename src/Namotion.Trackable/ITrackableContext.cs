using Namotion.Trackable.Model;
using System.Collections.Generic;

namespace Namotion.Trackable;

public interface ITrackableContext
{
    ProxyTracker? TryGetTracker(object proxy);

    IReadOnlyCollection<ProxyTracker> AllTrackers { get; }

    IReadOnlyCollection<TrackedProperty> AllProperties { get; }

    internal object Object { get; }

    internal void InitializeProxy(ITrackable proxy);

    internal void AttachPropertyValue(TrackedProperty property, object newValue);

    internal void DetachPropertyValue(TrackedProperty property, object? newValue);

    internal void MarkPropertyAsChanged(TrackedProperty setVariable);
}

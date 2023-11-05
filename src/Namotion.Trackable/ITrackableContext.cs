using Namotion.Trackable.Model;
using System.Collections.Generic;

namespace Namotion.Trackable;

public interface ITrackableContext : ITrackableFactory
{
    Tracker? TryGetTracker(object proxy);

    IReadOnlyCollection<Tracker> AllTrackers { get; }

    IReadOnlyCollection<TrackedProperty> AllProperties { get; }

    internal object Object { get; }

    internal void InitializeProxy(ITrackable proxy);

    internal void AttachPropertyValue(TrackedProperty property, object newValue);

    internal void DetachPropertyValue(TrackedProperty property);

    internal void MarkPropertyAsChanged(TrackedProperty setVariable);
}

using Namotion.Trackable.Model;
using System.Collections.Generic;

namespace Namotion.Trackable;

public interface ITrackableContext : ITrackableFactory
{
    Tracker? TryGetTracker(object proxy);

    IEnumerable<TrackedProperty> AllProperties { get; }

    internal object Object { get; }

    internal void InitializeProxy(ITrackable proxy);

    internal void AttachPropertyValue(TrackedProperty property, object newValue);

    internal void DetachPropertyValue(TrackedProperty property, object previousValue);

    internal void MarkPropertyAsChanged(TrackedProperty setVariable);
}

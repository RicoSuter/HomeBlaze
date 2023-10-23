using Namotion.Trackable.Model;
using System.Collections.Generic;

namespace Namotion.Trackable;

public interface ITrackableContext : ITrackableFactory
{
    IEnumerable<TrackedProperty> AllProperties { get; }

    IReadOnlyCollection<Tracker> Trackables { get; }

    internal object Object { get; }

    internal void InitializeProxy(object proxy);

    internal void TouchProxy(object proxy);

    internal void CreateTracker(object proxy, string parentPath, TrackedProperty? parentProperty, int? parentCollectionIndex);

    internal void Attach(TrackedProperty property, object newValue);

    internal void Detach(object previousValue);

    internal void MarkVariableAsChanged(TrackedProperty setVariable);
}

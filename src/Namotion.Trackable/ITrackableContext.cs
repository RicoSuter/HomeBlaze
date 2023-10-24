using Namotion.Trackable.Model;
using System.Collections.Generic;

namespace Namotion.Trackable;

public interface ITrackableContext : ITrackableFactory
{
    IEnumerable<TrackedProperty> AllProperties { get; }

    internal object Object { get; }

    internal void InitializeProxy(object proxy);

    internal void Attach(TrackedProperty property, object newValue);

    internal void Detach(object previousValue);

    internal void MarkVariableAsChanged(TrackedProperty setVariable);
}

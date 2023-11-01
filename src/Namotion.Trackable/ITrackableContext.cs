using Namotion.Trackable.Model;
using System.Collections.Generic;

namespace Namotion.Trackable;

public interface ITrackableContext
{
    IEnumerable<TrackedProperty> AllProperties { get; }

    internal object Object { get; }

    internal void InitializeProxy(ITrackable proxy);

    internal void Attach(TrackedProperty property, object newValue);

    internal void Detach(object previousValue);

    internal void MarkVariableAsChanged(TrackedProperty setVariable);
}

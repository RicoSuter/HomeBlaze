using Namotion.Trackable.Model;
using System.Collections.Generic;

namespace Namotion.Trackable;

public interface ITrackableContext
{
    IEnumerable<TrackableProperty> AllProperties { get; }

    internal object Object { get; }

    internal void Initialize(object obj);

    internal void Attach(object invocationTarget, object newValue);

    internal void Detach(object previousValue);

    internal void MarkVariableAsChanged(TrackableProperty setVariable, bool isChangingFromSource);
}

using Namotion.Trackable.Model;
using System.Collections.Generic;

namespace Namotion.Trackable;

public interface ITrackableContext : ITrackableFactory
{
    IEnumerable<TrackableProperty> AllProperties { get; }

    internal object Object { get; }

    internal void Initialize(object obj);

    internal IEnumerable<Model.Trackable> CreateThings(object proxy, string parentTargetPath, string? parentSourcePath, Model.Trackable? parent);

    internal void Attach(object invocationTarget, object newValue);

    internal void Detach(object previousValue);

    internal void MarkVariableAsChanged(TrackableProperty setVariable, bool isChangingFromSource);
}

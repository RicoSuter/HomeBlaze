using Namotion.Trackable.Model;
using System;
using System.Collections.Generic;

namespace Namotion.Trackable;

public interface ITrackableContext : IObservable<TrackedPropertyChange>, IObserver<TrackedPropertyChange>
{
    ProxyTracker? TryGetTracker(object proxy);

    IReadOnlyCollection<ProxyTracker> AllTrackers { get; }

    IReadOnlyCollection<TrackedProperty> AllProperties { get; }

    internal object Object { get; }

    internal void InitializeProxy(ITrackable proxy);

    internal void AttachPropertyValue(TrackedProperty property, object newValue);

    internal void DetachPropertyValue(TrackedProperty property, object? newValue);

    internal void RaisePropertyChanged(TrackedProperty setVariable);
}

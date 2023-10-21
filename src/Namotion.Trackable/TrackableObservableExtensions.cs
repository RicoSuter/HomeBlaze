using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

using Namotion.Trackable.Model;

namespace Namotion.Trackable;

public static class TrackableObservableExtensions
{
    public static IObservable<IEnumerable<TrackedPropertyChange>> BufferChanges(this IObservable<TrackedPropertyChange> observable, TimeSpan bufferTime)
    {
        return observable
            .Buffer(bufferTime)
            .Where(propertyChanges => propertyChanges.Any())
            .Select(propertyChanges => propertyChanges.Reverse().DistinctBy(c => c.Property));
    }
}

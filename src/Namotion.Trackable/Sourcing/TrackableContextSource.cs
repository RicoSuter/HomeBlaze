using Namotion.Trackable.Model;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Namotion.Trackable.Sourcing;

public abstract class TrackableContextSource<TTrackable>
    where TTrackable : class
{
    private readonly TrackableContext<TTrackable> _trackableContext;

    public TrackableContextSource(TrackableContext<TTrackable> trackableContext)
    {
        _trackableContext = trackableContext;
    }

    public abstract Task<IReadOnlyDictionary<string, object?>> ReadAsync(IEnumerable<string> sourcePaths, CancellationToken cancellationToken);

    public abstract Task<IDisposable> SubscribeAsync(IEnumerable<string> sourcePaths, CancellationToken cancellationToken);

    public abstract Task WriteAsync(IReadOnlyDictionary<string, object?> propertyChanges, CancellationToken cancellationToken);

    public void UpdatePropertyValueFromSource(string sourcePath, object? value)
    {
        var property = _trackableContext
            .AllProperties
            .FirstOrDefault(v => v.TryGetSourcePath() == sourcePath);

        if (property != null)
        {
            property.SetValueFromSource(value);
        }
    }
}

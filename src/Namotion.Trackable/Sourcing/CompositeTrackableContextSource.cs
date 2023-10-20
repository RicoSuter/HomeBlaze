using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Namotion.Trackable.Sourcing;

public class CompositeTrackableContextSource<TTrackable> : TrackableContextSource<TTrackable>
    where TTrackable : class
{
    private readonly IReadOnlyDictionary<string, TrackableContextSource<TTrackable>> _sources;

    public CompositeTrackableContextSource(IReadOnlyDictionary<string, TrackableContextSource<TTrackable>> sources, TrackableContext<TTrackable> trackableContext)
        : base(trackableContext)
    {
        _sources = sources
            .OrderByDescending(s => s.Key)
            .ToDictionary(s => s.Key, s => s.Value);
    }

    public override Task<IReadOnlyDictionary<string, object?>> ReadAsync(IEnumerable<string> sourcePaths, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task<IDisposable> SubscribeAsync(IEnumerable<string> sourcePaths, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task WriteAsync(IReadOnlyDictionary<string, object?> propertyChanges, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Namotion.Trackable.Sourcing;

public interface ITrackableSource
{
    Task<IDisposable?> InitializeAsync(string sourceName, IEnumerable<string> sourcePaths, Action<string, object?> propertyUpdateAction, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, object?>> ReadAsync(IEnumerable<string> sourcePaths, CancellationToken cancellationToken);

    Task WriteAsync(IReadOnlyDictionary<string, object?> propertyChanges, CancellationToken cancellationToken);
}

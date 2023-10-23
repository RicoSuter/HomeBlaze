using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Namotion.Trackable.Model;

namespace Namotion.Trackable.Sources;

public interface ITrackableSource
{
    string? TryGetSourcePath(TrackedProperty property);

    Task<IDisposable?> InitializeAsync(IEnumerable<string> sourcePaths, Action<string, object?> propertyUpdateAction, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, object?>> ReadAsync(IEnumerable<string> sourcePaths, CancellationToken cancellationToken);

    Task WriteAsync(IReadOnlyDictionary<string, object?> propertyChanges, CancellationToken cancellationToken);
}

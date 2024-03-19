using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Namotion.Trackable.Model;

namespace Namotion.Trackable.Sources;

public interface ITrackableSource
{
    string? TryGetSourcePath(TrackedProperty property);

    Task<IDisposable?> InitializeAsync(IEnumerable<PropertyInfo> properties, Action<PropertyInfo> propertyUpdateAction, CancellationToken cancellationToken);

    Task<IEnumerable<PropertyInfo>> ReadAsync(IEnumerable<PropertyInfo> properties, CancellationToken cancellationToken);

    Task WriteAsync(IEnumerable<PropertyInfo> propertyChanges, CancellationToken cancellationToken);
}

public record struct PropertyInfo(TrackedProperty Property, string Path, object? Value);

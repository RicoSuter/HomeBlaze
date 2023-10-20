using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Namotion.Trackable.Model;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Namotion.Trackable.Sourcing;

public abstract class TrackableContextSource<TVariables> : BackgroundService
    where TVariables : class
{
    private readonly TrackableContext<TVariables> _trackableContext;
    private readonly ILogger _logger;
    private readonly TimeSpan _bufferTime;

    public TrackableContextSource(
        TrackableContext<TVariables> trackableContext,
        ILogger logger,
        TimeSpan? bufferTime = null)
    {
        _trackableContext = trackableContext;
        _logger = logger;
        _bufferTime = bufferTime ?? TimeSpan.FromMilliseconds(8);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sourcePaths = _trackableContext
            .AllProperties
            .Where(p => _trackableContext.Trackables.Any(t => t.Parent == p) == false) // only properties with untracked values - TODO: is this a good idea?
            .Select(v => v.TryGetSourcePath())
            .Where(v => v is not null)
            .ToList();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var initialValues = await ReadAsync(sourcePaths!, stoppingToken);
                foreach (var value in initialValues)
                {
                    UpdatePropertyValueFromSource(value.Key, value.Value);
                }

                using var disposable = await SubscribeAsync(sourcePaths!, stoppingToken);

                await _trackableContext
                    .Where(change => !change.IsChangingFromSource() && change.Property.TryGetSourcePath() != null)
                    .BufferChanges(_bufferTime)
                    .Where(changes => changes.Any())
                    .ForEachAsync(async changes => await WriteAsync(changes, stoppingToken), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to listen for changes.");
                await Task.Delay(10000);
            }
        }
    }

    protected abstract Task<IReadOnlyDictionary<string, object?>> ReadAsync(IEnumerable<string> sourcePaths, CancellationToken cancellationToken);

    protected abstract Task<IDisposable> SubscribeAsync(IEnumerable<string> sourcePaths, CancellationToken cancellationToken);

    protected abstract Task WriteAsync(IEnumerable<TrackablePropertyChange> propertyChanges, CancellationToken cancellationToken);

    protected void UpdatePropertyValueFromSource(string sourcePath, object? value)
    {
        var variable = _trackableContext
            .AllProperties
            .FirstOrDefault(v => v.TryGetSourcePath() == sourcePath);

        if (variable != null)
        {
            variable.SetValueFromSource(value);
        }
    }
}

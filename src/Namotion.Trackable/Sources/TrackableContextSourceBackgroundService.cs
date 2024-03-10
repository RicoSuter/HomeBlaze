using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Generic;

namespace Namotion.Trackable.Sources;

public class TrackableContextSourceBackgroundService<TTrackable> : BackgroundService
    where TTrackable : class
{
    private readonly TrackableContext<TTrackable> _trackableContext;
    private readonly ITrackableSource _source;
    private readonly ILogger _logger;
    private readonly IToSourceConverter? _toSourceConverter;
    private readonly IFromSourceConverter? _fromSourceConverter;
    private readonly TimeSpan _bufferTime;
    private readonly TimeSpan _retryTime;

    private HashSet<string>? _initializedProperties;

    public TrackableContextSourceBackgroundService(
        ITrackableSource source,
        TrackableContext<TTrackable> trackableContext,
        ILogger logger,
        IToSourceConverter? toSourceConverter = null,
        IFromSourceConverter? fromSourceConverter = null,
        TimeSpan? bufferTime = null,
        TimeSpan? retryTime = null)
    {
        _source = source;
        _trackableContext = trackableContext;
        _logger = logger;
        _toSourceConverter = toSourceConverter;
        _fromSourceConverter = fromSourceConverter;
        _bufferTime = bufferTime ?? TimeSpan.FromMilliseconds(8);
        _retryTime = retryTime ?? TimeSpan.FromSeconds(10);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // TODO: Currently newly added properties/trackable are not automatically tracked/subscribed to

                var sourcePaths = _trackableContext
                    .AllProperties
                    .Select(_source.TryGetSourcePath)
                    .Where(p => p is not null)
                    .ToList();

                lock (this)
                {
                    _initializedProperties = new HashSet<string>();
                }

                // subscribe first and mark all properties as initialized which are updated before the read has completed 
                using var disposable = await _source.InitializeAsync(sourcePaths!, UpdatePropertyValueFromSource, stoppingToken);

                // read all properties (subscription during read will later be ignored)
                var initialValues = await _source.ReadAsync(sourcePaths!, stoppingToken);
                lock (this)
                {
                    // ignore properties which have been updated via subscription
                    foreach (var value in initialValues
                        .Where(v => !_initializedProperties.Contains(v.Key)))
                    {
                        UpdatePropertyValueFromSource(value.Key, value.Value);
                    }

                    _initializedProperties = null;
                }

                await _trackableContext
                    .Where(change => !change.IsChangingFromSource(_source) &&
                                     _source.TryGetSourcePath(change.Property) != null)
                    .BufferChanges(_bufferTime)
                    .Where(changes => changes.Any())
                    .ForEachAsync(async changes =>
                    {
                        var values = changes
                           .ToDictionary(
                               c => _source.TryGetSourcePath(c.Property)!,
                               c => _toSourceConverter is not null ? 
                                        _toSourceConverter.ConvertToSource(c.Property, c.Value) : 
                                        c.Value);

                        await _source.WriteAsync(values, stoppingToken);
                    }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to listen for changes.");
                await Task.Delay(_retryTime, stoppingToken);
            }
        }
    }

    protected void UpdatePropertyValueFromSource(string sourcePath, object? value)
    {
        MarkPropertyAsInitialized(sourcePath);

        var property = _trackableContext
            .AllProperties
            .FirstOrDefault(v => _source.TryGetSourcePath(v) == sourcePath);

        if (property is not null)
        {
            property.SetValueFromSource(_source, value, _fromSourceConverter);
        }
    }

    private void MarkPropertyAsInitialized(string sourcePath)
    {
        if (_initializedProperties is not null)
        {
            lock (this)
            {
                if (_initializedProperties != null)
                {
                    _initializedProperties.Add(sourcePath);
                }
            }
        }
    }
}

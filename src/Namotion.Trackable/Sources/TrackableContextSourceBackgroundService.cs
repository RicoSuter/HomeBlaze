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
    private readonly TimeSpan _bufferTime;
    private readonly TimeSpan _retryTime;

    private HashSet<string>? _initializedProperties;

    public TrackableContextSourceBackgroundService(
        ITrackableSource source,
        TrackableContext<TTrackable> trackableContext,
        ILogger logger,
        TimeSpan? bufferTime = null,
        TimeSpan? retryTime = null)
    {
        _source = source;
        _trackableContext = trackableContext;
        _logger = logger;
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

                var properties = _trackableContext
                    .AllProperties
                    .Where(p => _source.TryGetSourcePath(p) is not null)
                    .Select(p => new PropertyInfo(p, _source.TryGetSourcePath(p)!, p.GetValue()))
                    .ToList();

                lock (this)
                {
                    _initializedProperties = new HashSet<string>();
                }

                // subscribe first and mark all properties as initialized which are updated before the read has completed 
                using var disposable = await _source.InitializeAsync(properties!, UpdatePropertyValueFromSource, stoppingToken);

                // read all properties (subscription during read will later be ignored)
                var initialValues = await _source.ReadAsync(properties!, stoppingToken);
                lock (this)
                {
                    // ignore properties which have been updated via subscription
                    foreach (var value in initialValues
                        .Where(v => !_initializedProperties.Contains(v.Path)))
                    {
                        UpdatePropertyValueFromSource(value);
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
                            .Select(c => new PropertyInfo(c.Property, _source.TryGetSourcePath(c.Property)!, c.Value))
                            .ToList();

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

    protected void UpdatePropertyValueFromSource(PropertyInfo property2)
    {
        MarkPropertyAsInitialized(property2.Path);
        property2.Property.SetValueFromSource(_source, property2.Value);
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

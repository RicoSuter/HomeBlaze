using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Namotion.Trackable.Sourcing;

public class TrackableContextSourceBackgroundService<TTrackable> : BackgroundService
    where TTrackable : class
{
    private readonly TrackableContext<TTrackable> _trackableContext;
    private readonly TrackableContextSource<TTrackable> _source;
    private readonly ILogger _logger;

    private readonly TimeSpan _bufferTime;

    public TrackableContextSourceBackgroundService(
        TrackableContext<TTrackable> trackableContext,
        TrackableContextSource<TTrackable> source,
        ILogger logger,
        TimeSpan? bufferTime = null)
    {
        _trackableContext = trackableContext;
        _source = source;
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
                var initialValues = await _source.ReadAsync(sourcePaths!, stoppingToken);
                foreach (var value in initialValues)
                {
                    _source.UpdatePropertyValueFromSource(value.Key, value.Value);
                }

                using var disposable = await _source.SubscribeAsync(sourcePaths!, stoppingToken);

                await _trackableContext
                    .Where(change => !change.IsChangingFromSource() && change.Property.TryGetSourcePath() != null)
                    .BufferChanges(_bufferTime)
                    .Where(changes => changes.Any())
                    .ForEachAsync(async changes =>
                    {
                        var values = changes
                           .ToDictionary(
                               c => c.Property.TryGetSourcePath()!,
                               c => c.Property.ConvertToSource(c.Value));

                        await _source.WriteAsync(values, stoppingToken);
                    }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to listen for changes.");
                await Task.Delay(10000);
            }
        }
    }
}

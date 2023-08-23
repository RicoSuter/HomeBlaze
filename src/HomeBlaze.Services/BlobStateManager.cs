using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Namotion.Storage;
using System.Text.Json.Serialization;
using System.Text.Json;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Messages;
using HomeBlaze.Abstractions;

namespace HomeBlaze.Services
{
    public class BlobStateManager : BackgroundService, IDisposable, IStateManager
    {
        private static readonly JsonSerializerOptions _stateSerializerOptions = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly IBlobContainer _blobContainer;
        private readonly ILogger<BlobStateManager> _logger;

        private readonly IDisposable _eventSubscription;

        private List<(string, string)> queue = new List<(string, string)>();

        public BlobStateManager(IEventManager eventManager, IBlobContainer blobContainer, ILogger<BlobStateManager> logger)
        {
            _blobContainer = blobContainer;
            _logger = logger;

            _eventSubscription = eventManager.Subscribe(message =>
            {
                if (message is ThingStateChangedEvent stateChangedEvent)
                {
                    OnThingStateChanged(stateChangedEvent);
                }
            });
        }

        public override void Dispose()
        {
            _eventSubscription.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    IGrouping<string, (string, string)>[]? groups = null;
                    lock (this)
                    {
                        groups = queue
                            .GroupBy(q => q.Item1)
                            .ToArray();

                        queue.Clear();
                    }

                    foreach (var group in groups)
                    {
                        await _blobContainer.AppendTextAsync(group.Key,
                            string.Join("", group.Select(p => p.Item2)),
                            CancellationToken.None);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Failed to write state.");
                    // TODO: Should we add them back to queue and retry later?
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private void OnThingStateChanged(ThingStateChangedEvent stateChangedEvent)
        {
            try
            {
                if (stateChangedEvent.NewValue is not Image)
                {
                    var thingId = stateChangedEvent.Thing.Id;
                    if (thingId != null)
                    {
                        var fileName = GetFileName(thingId);
                        var date = stateChangedEvent.ChangeDate.ToString("yyyy-MM-dd");

                        lock (this)
                        {
                            queue.Add(("History/" + date + "/" + fileName + "/" + stateChangedEvent.PropertyName + ".txt",
                                stateChangedEvent.ChangeDate.ToString("O") + "\t" + JsonSerializer.Serialize(stateChangedEvent.NewValue, _stateSerializerOptions) + "\n"));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to store state.");
            }
        }

        public async Task<ValueTuple<DateTimeOffset, TState?>[]> ReadStateAsync<TState>(
            string thingId, string propertyName,
            DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
        {
            try
            {
                var list = new List<ValueTuple<DateTimeOffset, TState?>>();

                var tasks = new List<Task>();
                var day = from.Date;
                do
                {
                    var date = day.ToString("yyyy-MM-dd");
                    tasks.Add(Task.Run(async () =>
                    {
                        thingId = GetFileName(thingId);

                        var path = "History/" + date + "/" + thingId + "/" + propertyName + ".txt";
                        if (await _blobContainer.ExistsAsync(path, cancellationToken))
                        {
                            var text = await _blobContainer.ReadAllTextAsync(path, cancellationToken);
                            foreach (var line in text.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)))
                            {
                                try
                                {
                                    var pair = line.Split('\t');

                                    var updated = DateTimeOffset.Parse(pair[0]);
                                    if (updated >= from && updated < to)
                                    {
                                        var value = JsonSerializer.Deserialize<TState>(pair[1]);
                                        lock (list)
                                        {
                                            list.Add(new ValueTuple<DateTimeOffset, TState?>(updated, value));
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger.LogWarning(e, "Failed to read state line.");
                                }
                            }
                        }
                    }));
                    day = day.AddDays(1);
                } while (day < to);

                await Task.WhenAll(tasks);

                return list
                    .OrderBy(l => l.Item1)
                    .ToArray();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to read state.");
                return Array.Empty<(DateTimeOffset, TState?)>();
            }
        }

        private static string GetFileName(string name)
        {
            var invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalidChars)
            {
                name = name.Replace(c.ToString(), "_");
            }

            return name;
        }
    }
}
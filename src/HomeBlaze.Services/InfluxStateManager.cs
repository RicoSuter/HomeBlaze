using Microsoft.Extensions.Logging;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Messages;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HomeBlaze.Services
{
    public class InfluxStateManager : BackgroundService, IDisposable, IStateManager
    {
        private readonly InfluxDBClient _client;
        private readonly WriteApi _writeApi;
        private readonly ILogger<InfluxStateManager> _logger;
        private readonly IDisposable _eventSubscription;

        private readonly string? _url;
        private readonly string? _token;
        private readonly string? _bucket;
        private readonly string? _organization;

        public InfluxStateManager(IEventManager eventManager, IConfiguration configuration, ILogger<InfluxStateManager> logger)
        {
            _url = configuration.GetValue<string>("Series:Url");
            _token = configuration.GetValue<string>("Series:Token");
            _bucket = configuration.GetValue("Series:Bucket", "HomeBlaze");
            _organization = configuration.GetValue("Series:Organization", "HomeBlaze");

            _client = new InfluxDBClient(_url, _token);
            _writeApi = _client.GetWriteApi();
            _logger = logger;

            _eventSubscription = eventManager.Subscribe(message =>
            {
                if (message is ThingStateChangedEvent stateChangedEvent)
                {
                    OnThingStateChanged(stateChangedEvent);
                }
            });
        }

        public async Task<ValueTuple<DateTimeOffset, TState?>[]> ReadStateAsync<TState>(
            string thingId, string propertyName,
            DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
        {
            try
            {
                var flux = "from(bucket:\"" + _bucket + "\")\r\n" +
                    "|> range(start: " + from.ToUniversalTime().ToString("o") + ", " +
                             "stop: " + to.ToUniversalTime().ToString("o") + ")\r\n" +
                    "|> filter(fn: (r) => r[\"_measurement\"] == \"" + thingId + "\")\r\n" +
                    "|> filter(fn: (r) => r[\"_field\"] == \"" + propertyName + "\")";

                var list = new List<ValueTuple<DateTimeOffset, TState?>>();
                var fluxTables = await _client.GetQueryApi().QueryAsync(flux, _organization, cancellationToken);
                fluxTables.ForEach(fluxTable =>
                {
                    var fluxRecords = fluxTable.Records;
                    fluxRecords.ForEach(fluxRecord =>
                    {
                        var time = fluxRecord.GetTime();
                        if (time.HasValue)
                        {
                            list.Add(new ValueTuple<DateTimeOffset, TState?>(
                                time.Value.ToDateTimeOffset().ToLocalTime(),
                                (TState)fluxRecord.GetValue()));
                        }
                    });
                });

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

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        private void OnThingStateChanged(ThingStateChangedEvent stateChangedEvent)
        {
            try
            {
                var newValue = stateChangedEvent.NewValue;
                if (newValue is not byte[])
                {
                    var thingId = stateChangedEvent.Thing.Id;
                    if (thingId != null && newValue != null)
                    {
                        PublishState(
                            thingId,
                            stateChangedEvent.PropertyName,
                            newValue,
                            stateChangedEvent.ChangeDate,
                            new Dictionary<string, string?>
                            {
                                { "Type", stateChangedEvent.Thing.GetType().FullName },
                                { "Title", stateChangedEvent.Thing.Title }
                            }
                        );
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to store state.");
            }
        }

        public void PublishState(
            string thingId,
            string? propertyName,
            object value,
            DateTimeOffset changeDate,
            IReadOnlyDictionary<string, string?>? tags)
        {
            var point = PointData
                .Measurement(thingId)
                .Field(propertyName,
                    value is Enum ? value.ToString() :
                    value is TimeSpan timeSpan ? timeSpan.TotalSeconds :
                    value)
                .Timestamp(
                    changeDate.ToUniversalTime(),
                    WritePrecision.Ns);

            if (tags is not null)
            {
                foreach (var tag in tags.Where(p => p.Value is not null))
                {
                    point = point.Tag(tag.Key, tag.Value);
                }
            }

            _writeApi.WritePoint(point, _bucket, _organization);
        }

        public override void Dispose()
        {
            _eventSubscription.Dispose();
            _client.Dispose();
            base.Dispose();
        }
    }
}
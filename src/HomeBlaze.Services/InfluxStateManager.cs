using Microsoft.Extensions.Logging;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Messages;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Configuration;
using HomeBlaze.Services.Abstractions;
using HomeBlaze.Abstractions.Messages;

namespace HomeBlaze.Services
{
    public class InfluxStateManager : AsyncEventListener, IDisposable, IStateManager
    {
        private readonly InfluxDBClient _client;
        private readonly WriteApiAsync _writeApi;
        private readonly ILogger<InfluxStateManager> _logger;

        private readonly string? _url;
        private readonly string? _username;
        private readonly string? _password;
        private readonly string? _bucket;
        private readonly string? _organization;

        public InfluxStateManager(IEventManager eventManager, IConfiguration configuration, ILogger<InfluxStateManager> logger)
            : base(eventManager)
        {
            _url = configuration.GetValue<string>("Series:Url");
            _username = configuration.GetValue<string>("Series:Username");
            _password = configuration.GetValue<string>("Series:Password");
            _bucket = configuration.GetValue("Series:Bucket", "HomeBlaze");
            _organization = configuration.GetValue("Series:Organization", "HomeBlaze");

            _client = new InfluxDBClient(_url, _username, _password);
            _writeApi = _client.GetWriteApiAsync();
            _logger = logger;
        }

        protected override async Task HandleMessageAsync(IEvent message, CancellationToken cancellationToken)
        {
            if (message is ThingStateChangedEvent stateChangedEvent)
            {
                await OnThingStateChangedAsync(stateChangedEvent, cancellationToken);
            }
        }

        private async Task OnThingStateChangedAsync(ThingStateChangedEvent stateChangedEvent, CancellationToken cancellationToken)
        {
            try
            {
                var newValue = stateChangedEvent.NewValue;
                if (newValue is not byte[])
                {
                    var thingId = stateChangedEvent.Thing.Id;
                    if (thingId != null && newValue != null)
                    {
                        var point = PointData
                            .Measurement(thingId)
                            .Tag("Type", stateChangedEvent.Thing.GetType().FullName)
                            .Tag("Title", stateChangedEvent.Thing.Title)
                            .Field(stateChangedEvent.PropertyName, newValue)
                            .Timestamp(
                                stateChangedEvent.ChangeDate.ToUniversalTime(),
                                WritePrecision.Ns);

                        await _writeApi.WritePointAsync(point, _bucket, _organization);
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

        public override void Dispose()
        {
            _client.Dispose();
            base.Dispose();
        }
    }
}
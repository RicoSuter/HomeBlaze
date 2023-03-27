using Microsoft.Extensions.Logging;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Messages;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Configuration;

namespace HomeBlaze.Services
{
    public class InfluxStateManager : IDisposable, IStateManager
    {
        private readonly InfluxDBClient _client;
        private readonly WriteApi _writeApi;

        private readonly ILogger<InfluxStateManager> _logger;

        private readonly IDisposable _eventSubscription;

        private readonly string? _url;
        private readonly string? _username;
        private readonly string? _password;
        private readonly string? _bucket;
        private readonly string? _organization;

        public InfluxStateManager(IEventManager eventManager, IConfiguration configuration, ILogger<InfluxStateManager> logger)
        {
            _url = configuration.GetValue<string>("Series:Url");
            _username = configuration.GetValue<string>("Series:Username");
            _password = configuration.GetValue<string>("Series:Password");
            _bucket = configuration.GetValue("Series:Bucket", "HomeBlaze");
            _organization = configuration.GetValue("Series:Organization", "HomeBlaze");

            _client = new InfluxDBClient(_url, _username, _password);
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

        private void OnThingStateChanged(ThingStateChangedEvent stateChangedEvent)
        {
            try
            {
                if (stateChangedEvent.NewValue is not byte[])
                {
                    var thingId = stateChangedEvent.Thing.Id;
                    if (thingId != null)
                    {
                        var point = PointData
                            .Measurement("property")
                            .Tag("property", stateChangedEvent.PropertyName)
                            .Tag("thing", stateChangedEvent.Thing.Id)
                            .Field("value", stateChangedEvent.NewValue)
                            .Timestamp(
                                stateChangedEvent.ChangeDate.ToUniversalTime(),
                                WritePrecision.Ns);

                        _writeApi.WritePoint(point, _bucket, _organization);
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
                    "|> filter(fn: (r) => r[\"thing\"] == \"" + thingId + "\")\r\n" +
                    "|> filter(fn: (r) => r[\"property\"] == \"" + propertyName + "\")";

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

        public void Dispose()
        {
            _eventSubscription.Dispose();
            _writeApi.Dispose();
            _client.Dispose();
        }
    }
}
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Things;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Tesla.WallConnector
{
    [DisplayName("Tesla Wall Connector")]
    [ThingSetup(typeof(TeslaWallConnectorSetup), CanEdit = true)]
    public class TeslaWallConnector : PollingThing, IIconProvider, IConnectedDevice, IPowerConsumptionSensor, IVehicleCharger
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        [JsonIgnore]
        public override string? Id => SerialNumber != null ? "tesla.wall.charger." + SerialNumber : null;

        [JsonIgnore]
        public override string Title => "Tesla Wall Connector (" + (SerialNumber?.ToString() ?? "?") + ")";

        public string IconName => "fas fa-plug";

        // State

        [JsonIgnore]
        public bool IsConnected { get; private set; }

        [JsonIgnore]
        public bool? IsPluggedIn => Vitals?.IsVehicleConnected;

        [JsonIgnore]
        public bool? IsCharging => PowerConsumption > 0;

        [JsonIgnore]
        public decimal? PowerConsumption
        {
            get
            {
                var powerConsumption =
                    CurrentA * VoltageA +
                    CurrentB * VoltageB +
                    CurrentC * VoltageC;

                return powerConsumption.HasValue ?
                    Math.Round(powerConsumption.Value, 1) :
                    null;
            }
        }

        [State(Unit = StateUnit.Ampere)]
        public decimal? CurrentA => (decimal?)Vitals?.CurrentA_a;

        [State(Unit = StateUnit.Ampere)]
        public decimal? CurrentB => Vitals?.CurrentB_a;

        [State(Unit = StateUnit.Ampere)]
        public decimal? CurrentC => Vitals?.CurrentC_a;

        [State(Unit = StateUnit.Volt)]
        public decimal? VoltageA => Vitals?.VoltageA_v;

        [State(Unit = StateUnit.Volt)]
        public decimal? VoltageB => Vitals?.VoltageB_v;

        [State(Unit = StateUnit.Volt)]
        public decimal? VoltageC => Vitals?.VoltageC_v;

        [State(Unit = StateUnit.Watt, IsAggregation = true)]
        public double? TotalConsumedPower => Lifetime?.TotalConsumedPower;

        [State(Unit = StateUnit.Volt)]
        public decimal? GridVoltage => Vitals?.Grid_v;

        [State]
        public decimal? GridFrequency => Vitals?.Grid_hz;

        [State]
        public string? SerialNumber => Version?.SerialNumber;

        [State]
        public string? PartNumber => Version?.PartNumber;

        [State]
        public string? FirmwareVersion => Version?.FirmwareVersion;

        [State]
        public DateTimeOffset? StartupTime { get; private set; }

        [Configuration]
        public string? Host { get; set; }

        [Configuration]
        public int RefreshInterval { get; set; } = 30 * 1000;

        public TeslaWallConnectorVersion? Version { get; private set; }

        public TeslaWallConnectorVitals? Vitals { get; private set; }

        public TeslaWallConnectorLifetime? Lifetime { get; private set; }

        protected override TimeSpan PollingInterval => TimeSpan.FromMilliseconds(RefreshInterval);

        protected override TimeSpan FailureInterval => TimeSpan.FromSeconds(5);

        public TeslaWallConnector(IThingManager thingManager, IHttpClientFactory httpClientFactory, ILogger<TeslaWallConnector> logger) 
            : base(thingManager, logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    // https://teslamotorsclub.com/tmc/threads/gen3-wall-connector-api.228034/

                    var versionResponse = await httpClient.GetAsync($"http://{Host}/api/1/version", cancellationToken);
                    Version = JsonSerializer.Deserialize<TeslaWallConnectorVersion>(
                        await versionResponse.Content.ReadAsStringAsync(cancellationToken));

                    var vitalsResponse = await httpClient.GetAsync($"http://{Host}/api/1/vitals", cancellationToken);
                    Vitals = JsonSerializer.Deserialize<TeslaWallConnectorVitals>(
                        await vitalsResponse.Content.ReadAsStringAsync(cancellationToken));

                    var lifetimeResponse = await httpClient.GetAsync($"http://{Host}/api/1/lifetime", cancellationToken);
                    Lifetime = JsonSerializer.Deserialize<TeslaWallConnectorLifetime>(
                        await lifetimeResponse.Content.ReadAsStringAsync(cancellationToken));

                    StartupTime = Lifetime?.UptimeSeconds != null ?
                        DateTimeOffset.Now.AddMinutes(-Lifetime.UptimeSeconds / 60) :
                        null;

                    IsConnected = true;
                    _logger.LogDebug("Tesla Wall Connector refreshed.");

                    break;
                }
                catch (Exception exception)
                {
                    IsConnected = false;
                    _logger.LogError(exception, "Failed to refresh Tesla Wall Connector.");

                    await Task.Delay(FailureInterval, cancellationToken);
                }
            }
        }
    }
}
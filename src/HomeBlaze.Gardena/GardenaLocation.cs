using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Security;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Gardena
{
    [DisplayName("Gardena Location")]
    public class GardenaLocation : 
        PollingThing, 
        IIconProvider, IConnectedThing,
        IAuthenticatedThing, IHubDevice, 
        IPowerConsumptionSensor
    {
        private readonly ILogger _logger;

        private DateTimeOffset _lastRefresh = DateTimeOffset.MinValue;

        public DateTimeOffset? LastUpdated { get; private set; }

        internal Location? Location { get; private set; }

        public override string? Title => $"Gardena Location ({Location?.Name?.ToString() ?? "?"})";

        public string IconName => "fab fa-hubspot";

        internal IEnumerable<GardenaDevice> AllDevices => Devices.Concat(Devices.SelectMany(g => g.Children));

        IEnumerable<IThing> IHubDevice.Devices => Devices.OfType<IThing>();

        [State]
        public GardenaDevice[] Devices { get; private set; } = Array.Empty<GardenaDevice>();

        [State]
        public bool IsConnected => GardenaSocket?.WebSocket?.State == WebSocketState.Open;

        [State]
        public bool IsAuthenticated { get; internal set; }

        public decimal? PowerConsumption => 2.5m;

        [Configuration]
        public string? Username { get; set; }

        [Configuration(IsSecret = true)]
        public string? Password { get; set; }

        [Configuration]
        public string? ClientId { get; set; } = "288ee657-3d33-44cc-ad78-0559cbdd3bee";

        [Configuration]
        public string? LocationId { get; set; }

        internal GardenaRestClient? GardenaClient { get; private set; }

        internal GardenaWebSocketClient? GardenaSocket { get; private set; }

        protected override TimeSpan PollingInterval => TimeSpan.FromMinutes(2);

        protected override TimeSpan FailureInterval => TimeSpan.FromMinutes(15);

        public GardenaLocation(ILogger<GardenaLocation> logger) : base(logger)
        {
            _logger = logger;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            if (GardenaClient == null)
            {
                GardenaClient = new GardenaRestClient(ClientId!, Username!, Password!);
                GardenaSocket = new GardenaWebSocketClient(this, _logger);
            }

            GardenaSocket?.StartWebSocket(cancellationToken);

            if (IsConnected)
            {
                // keep alive
                await GardenaSocket!.WebSocket!.SendAsync(Encoding.UTF8.GetBytes("{}"), WebSocketMessageType.Text, true, CancellationToken.None);
                _logger.LogInformation("Gardena websocket kept alive.");
            }
            else
            {
                await RefreshAsync(cancellationToken);
            }
        }

        [Operation]
        public async Task<bool> RefreshAsync(CancellationToken cancellationToken)
        {
            if (DateTimeOffset.Now - _lastRefresh < TimeSpan.FromMinutes(60) || GardenaClient == null || LocationId == null)
            {
                return false;
            }

            for (int i = 0; i < 3; i++)
            {
                _logger.LogInformation("Refreshing Gardena location...");
                try
                {
                    var json = await GardenaClient.GetLocationAsync(LocationId, cancellationToken);

                    Location = new[] { json?["data"] as JObject }
                       .Where(e => e?["type"]?.Value<string>() == "LOCATION" &&
                                   e?["id"]?.Value<string>() == LocationId)
                       .Select(e => new Location
                       {
                           Id = e?["id"]?.Value<string>(),
                           Name = e?["attributes"]?["name"]?.Value<string>()
                       })
                       .FirstOrDefault();

                    var sensors = json?
                        ["included"]?
                        .OfType<JObject>()
                        .Where(e => e["type"]?.Value<string>() == "SENSOR" &&
                                    e["id"]?.Value<string>() != null)
                        .Select(e => Devices
                            .OfType<GardenaSensor>()
                            .SingleOrDefault(u => u.GardenaId == e["id"]?.Value<string>())?.Update(e)
                            ?? new GardenaSensor(this, e))
                        .ToArray() ?? Array.Empty<GardenaDevice>();

                    var irrigationControls = json?
                       ["included"]?
                       .OfType<JObject>()
                       .Where(e => e["type"]?.Value<string>() == "COMMON" &&
                                   e["attributes"]?["modelType"]?["value"]?.Value<string>() == "GARDENA smart Irrigation Control" &&
                                   e["id"]?.Value<string>() != null)
                       .Select(e => Devices
                           .OfType<GardenaIrrigationControl>()
                           .SingleOrDefault(u => u.GardenaId == e["id"]?.Value<string>())?.UpdateCommon(e) as GardenaIrrigationControl
                           ?? new GardenaIrrigationControl(this, e))
                       .ToArray()! ?? Array.Empty<GardenaIrrigationControl>();

                    var valves = json?
                        ["included"]?
                        .OfType<JObject>()
                        .Where(e => e["type"]?.Value<string>() == "VALVE" &&
                                    e["id"]?.Value<string>() != null)
                        .Select(e => Devices
                            .OfType<GardenaValve>()
                            .SingleOrDefault(u => u.GardenaId == e["id"]?.Value<string>())?.Update(e)
                            ?? new GardenaValve(this, e))
                        .ToArray() ?? Array.Empty<GardenaDevice>();

                    var devices = sensors
                        .Concat(valves)
                        .Concat(irrigationControls)
                        .OfType<GardenaDevice>()
                        .ToArray();

                    // Update common
                    var allDevices = json?
                        ["included"]?
                        .OfType<JObject>()
                        .Where(e => e["type"]?.Value<string>() == "COMMON" &&
                                    e["id"]?.Value<string>() != null)
                        .Select(e => devices.SingleOrDefault(u => u.GardenaId == e["id"]?.Value<string>())?.UpdateCommon(e))
                        .Where(e => e != null)
                        .Concat(devices)
                        .Distinct()
                        .ToArray()! ?? Array.Empty<GardenaDevice>();

                    foreach (var irrigationControl in irrigationControls)
                    {
                        irrigationControl.Valves = valves
                            .Where(v => v.GardenaId?.StartsWith(irrigationControl.GardenaId + ":") == true)
                            .ToArray();
                    }
                    var childValves = irrigationControls
                        .SelectMany(c => c.Valves)
                        .ToArray();

                    Devices = allDevices
                        .Where(v => !childValves.Contains(v))
                        .ToArray()!;

                    LastUpdated = DateTimeOffset.Now;
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Failed to refresh Gardena Location.");
                    await Task.Delay(FailureInterval, cancellationToken);
                }
            }

            _lastRefresh = DateTimeOffset.Now;
            return true;
        }

        public override void Dispose()
        {
            GardenaSocket?.Dispose();
            GardenaSocket = null;
        }       
    }
}

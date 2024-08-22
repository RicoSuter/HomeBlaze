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
    public class GardenaLocation : PollingThing, IIconProvider, IConnectedThing, IAuthenticatedThing, IHubDevice, IPowerConsumptionSensor
    {
        private ClientWebSocket? _webSocket;

        private bool _isRunning = false;
        private DateTimeOffset _lastRefresh = DateTimeOffset.MinValue;
        private readonly ILogger _logger;

        public DateTimeOffset? LastUpdated { get; private set; }

        internal Location? Location { get; private set; }

        public override string? Title => $"Gardena Location ({Location?.Name?.ToString() ?? "?"})";

        public string IconName => "fab fa-hubspot";

        internal IEnumerable<GardenaDevice> AllDevices => Devices.Concat(Devices.SelectMany(g => g.Children));

        IEnumerable<IThing> IHubDevice.Devices => Devices.OfType<IThing>();

        [State]
        public GardenaDevice[] Devices { get; private set; } = Array.Empty<GardenaDevice>();

        [State]
        public bool IsConnected => _webSocket?.State == WebSocketState.Open;

        [State]
        public bool IsAuthenticated { get; private set; }

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
            }

            TryStartBackgroundTask();

            if (IsConnected)
            {
                // keep alive
                await _webSocket!.SendAsync(Encoding.UTF8.GetBytes("{}"), WebSocketMessageType.Text, true, CancellationToken.None);
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

                    var webSocketAddress = await GardenaClient.GetWebSocketAddressAsync(LocationId, cancellationToken);
                    if (webSocketAddress != null)
                    {
                        try
                        {
                            _webSocket = new ClientWebSocket();
                            await _webSocket.ConnectAsync(new Uri(webSocketAddress), cancellationToken);
                        }
                        catch (Exception e)
                        {
                            _logger.LogWarning(e, "Failed to open websocket.");
                            _webSocket?.Dispose();
                            _webSocket = null;
                        }
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


        private void TryStartBackgroundTask()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Task.Run(async () =>
                {
                    while (_isRunning)
                    {
                        if (_webSocket != null)
                        {
                            if (_webSocket.State == WebSocketState.Open)
                            {
                                try
                                {
                                    var buffer = WebSocket.CreateClientBuffer(10 * 1024, 10 * 1024);
                                    var result = await _webSocket.ReceiveAsync(buffer, CancellationToken.None);
                                    if (result.Count > 0)
                                    {
                                        var json = Encoding.UTF8.GetString(buffer.Array!, 0, result.Count);
                                        _logger.LogInformation("Gardena websocket message received: {Json}.", json);

                                        try
                                        {
                                            var jObj = JObject.Parse(json);

                                            var gardenaId = jObj["id"]?.Value<string>();
                                            var type = jObj["type"]?.Value<string>();
                                            if (!string.IsNullOrEmpty(gardenaId) && !string.IsNullOrEmpty(type))
                                            {
                                                var device = AllDevices.FirstOrDefault(d => d.GardenaId == gardenaId);
                                                if (device != null)
                                                {
                                                    if (type == "COMMON")
                                                    {
                                                        device.UpdateCommon(jObj);
                                                    }
                                                    else
                                                    {
                                                        device.Update(jObj);
                                                    }

                                                    DetectChanges(device);
                                                }
                                            }

                                            IsAuthenticated = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            _logger.LogError(ex, "Failed to parse websocket message.");
                                            IsAuthenticated = false;
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, "Failed to receive websocket message.");
                                    await Task.Delay(1000);
                                    IsAuthenticated = false;
                                }
                            }
                            else if (_webSocket.State == WebSocketState.Closed)
                            {
                                IsAuthenticated = false;
                                _webSocket.Dispose();
                                _webSocket = null;
                            }
                            else
                            {
                                await Task.Delay(1000);
                            }

                            DetectChanges(this);
                        }
                        else
                        {
                            await Task.Delay(1000);
                        }
                    }
                });
            }
        }

        public override void Dispose()
        {
            _webSocket?.Dispose();
            _webSocket = null;
            _isRunning = false;
        }
    }
}

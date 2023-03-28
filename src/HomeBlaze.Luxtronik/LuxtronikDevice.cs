using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Security;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HomeBlaze.Luxtronik
{
    [DisplayName("Luxtronik Device")]
    [ThingSetup(typeof(LuxtronikDeviceSetup), CanEdit = true)]
    public class LuxtronikDevice : BackgroundService,
        IConnectedThing, IAuthenticatedThing, IPowerConsumptionSensor,
        IIconProvider, ILastUpdatedProvider
    {
        private readonly IThingManager _thingManager;
        private readonly ILogger _logger;

        private ClientWebSocket? _webSocket;
        private bool _isRunning = true;
        private bool _isInitialized = false;
        private XDocument? _metadataXml;

        public string? Title => DisplayTitle;

        public string IconName => "fa-solid fa-fire";

        public DateTimeOffset? LastUpdated { get; private set; }


        [Configuration(IsIdentifier = true)]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration("title")]
        public string? DisplayTitle { get; set; }

        [Configuration]
        public string? Host { get; set; }

        [Configuration]
        public int? Port { get; set; } = 8214;

        [Configuration(IsSecret = true)]
        public string? Password { get; set; }


        [State]
        public bool IsConnected =>
            _webSocket?.State == WebSocketState.Open &&
            (DateTimeOffset.Now - LastUpdated < TimeSpan.FromMinutes(1));

        [State]
        public bool IsAuthenticated { get; private set; }


        [State]
        public string? HeatPumpType { get; private set; }

        [State]
        public string? SoftwareVersion { get; private set; }

        [State]
        public string? OperationMode { get; private set; }

        [State(Unit = StateUnit.Watt, IsEstimated = true)]
        public decimal? PowerConsumption { get; set; }


        [State]
        public LuxtronikTemperature OutsideTemperature { get; private set; }

        [State]
        public LuxtronikTemperature WaterTemperature { get; private set; }

        [State]
        public LuxtronikTemperature FlowTemperature { get; private set; }

        [State]
        public LuxtronikTemperature ReturnTemperature { get; private set; }

        [State]
        public LuxtronikTemperature HeatSourceInletTemperature { get; private set; }

        [State]
        public LuxtronikTemperature HeatSourceOutletTemperature { get; private set; }


        [State(Unit = StateUnit.Watt)]
        public decimal? PowerProduction { get; private set; }


        [State(Unit = StateUnit.WattHour, IsCumulative = true)]
        public decimal? TotalProducedHeatEnergy { get; private set; }

        [State(Unit = StateUnit.WattHour, IsCumulative = true)]
        public decimal? TotalProducedWaterEnergy { get; private set; }

        [State(Unit = StateUnit.WattHour, IsCumulative = true)]
        public decimal? TotalProducedEnergy { get; private set; }


        [State(Unit = StateUnit.WattHour, IsCumulative = true)]
        public decimal? TotalConsumedHeatEnergy { get; private set; }

        [State(Unit = StateUnit.WattHour, IsCumulative = true)]
        public decimal? TotalConsumedWaterEnergy { get; private set; }

        [State(Unit = StateUnit.WattHour, IsCumulative = true)]
        public decimal? TotalConsumedEnergy { get; private set; }


        [State]
        public decimal? TotalCoefficientOfPerformance
        {
            get
            {
                var ratio = TotalProducedEnergy / TotalConsumedEnergy;
                return ratio != null ? Math.Round(ratio.Value, 2) : null;
            }
        }

        [State(Unit = StateUnit.LiterPerHour)]
        public decimal? FlowRate { get; private set; }

        public LuxtronikDevice(IThingManager thingManager, ILogger<LuxtronikDevice> logger)
        {
            _thingManager = thingManager;
            _logger = logger;

            OutsideTemperature = new LuxtronikTemperature(this, "OutsideTemperature") { Title = "Outside Temperature" };
            WaterTemperature = new LuxtronikTemperature(this, "WaterTemperature") { Title = "Water Temperature" };
            FlowTemperature = new LuxtronikTemperature(this, "FlowTemperature") { Title = "Flow Temperature" };
            ReturnTemperature = new LuxtronikTemperature(this, "ReturnTemperature") { Title = "Return Temperature" };
            HeatSourceInletTemperature = new LuxtronikTemperature(this, "HeatSourceInletTemperature") { Title = "Heat Source Inlet Temperature" };
            HeatSourceOutletTemperature = new LuxtronikTemperature(this, "HeatSourceOutletTemperature") { Title = "Heat Source Outlet Temperature" };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var buffer = WebSocket.CreateClientBuffer(10 * 1024, 10 * 1024);
            while (_isRunning && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_webSocket == null)
                    {
                        if (Host != null)
                        {
                            _webSocket = new ClientWebSocket();
                            _webSocket.Options!.AddSubProtocol("Lux_WS");
                            _isInitialized = false;

                            await _webSocket.ConnectAsync(new Uri("ws://" + Host + ":" + Port), stoppingToken);
                            await _webSocket.SendAsync(Encoding.UTF8.GetBytes("LOGIN;" + Password), WebSocketMessageType.Text, true, stoppingToken);
                            await _webSocket.SendAsync(Encoding.UTF8.GetBytes("REFRESH"), WebSocketMessageType.Text, true, stoppingToken);
                        }

                        await Task.Delay(1000, stoppingToken);
                    }
                    else
                    {
                        if (_webSocket.State == WebSocketState.Open)
                        {
                            try
                            {
                                if (!_isInitialized)
                                {
                                    var result = await _webSocket.ReceiveAsync(buffer, stoppingToken);
                                    if (result.Count > 0)
                                    {
                                        _isInitialized = true;

                                        var navigationXml = Encoding.UTF8.GetString(buffer.Array!, 0, result.Count);
                                        var navigationXmlDocument = XDocument.Parse(navigationXml);
                                        var informationId = navigationXmlDocument.Root?.Elements().First().Attribute("id")?.Value;

                                        await _webSocket.SendAsync(Encoding.UTF8.GetBytes("GET;" + informationId), WebSocketMessageType.Binary, true, stoppingToken);

                                        result = await _webSocket.ReceiveAsync(buffer, stoppingToken);
                                        if (result.Count > 0)
                                        {
                                            var xml = Encoding.UTF8.GetString(buffer.Array!, 0, result.Count);
                                            _metadataXml = XDocument.Parse(xml);
                                        }
                                    }
                                }
                                else
                                {
                                    await _webSocket.SendAsync(Encoding.UTF8.GetBytes("REFRESH"), WebSocketMessageType.Text, true, stoppingToken);
                                    var result = await _webSocket.ReceiveAsync(buffer, stoppingToken);
                                    if (result.Count > 0)
                                    {
                                        var xml = Encoding.UTF8.GetString(buffer.Array!, 0, result.Count);
                                        var xmlDocument = XDocument.Parse(xml);

                                        ProcessValues(xmlDocument);
                                    }
                                }
                                IsAuthenticated = true;
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, "Failed to receive websocket message.");
                                IsAuthenticated = false;
                            }
                        }
                        else if (_webSocket.State == WebSocketState.Closed ||
                                 _webSocket.State == WebSocketState.Aborted)
                        {
                            IsAuthenticated = false;
                            _webSocket.Dispose();
                            _webSocket = null;
                        }

                        _thingManager.DetectChanges(this);
                        await Task.Delay(10000, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failure in Luxtronic device.");

                    IsAuthenticated = false;
                    _webSocket?.Dispose();
                    _webSocket = null;

                    _thingManager.DetectChanges(this);
                    await Task.Delay(10000, stoppingToken);
                }
            }
        }

        private void ProcessValues(XDocument xmlDocument)
        {
            try
            {
                var allValues = xmlDocument.Root?.Elements("item")
                    .SelectMany(e => e.Elements("item").Concat(e.Elements("item").SelectMany(u => u.Elements("item"))))
                    .ToArray();

                var sections = _metadataXml?.Root?.Elements("item");

                var temperatures = sections?.ElementAt(0).Elements("item");
                var incoming = sections?.ElementAt(1).Elements("item");
                var state = sections?.ElementAt(7).Elements("item");

                var energy = sections?.ElementAt(8).Elements("item");
                var heatEnergy = energy?.ElementAt(0).Elements("item");
                var powerEnergy = energy?.ElementAt(1).Elements("item");

                HeatPumpType = GetString(allValues, state, 0);
                SoftwareVersion = GetString(allValues, state, 1);
                OperationMode = GetString(allValues, state, 7);
                PowerProduction = GetDecimal(allValues, state, 8) * 1000;

                FlowTemperature.Temperature = GetDecimal(allValues, temperatures, 0);
                ReturnTemperature.Temperature = GetDecimal(allValues, temperatures, 1);
                OutsideTemperature.Temperature = GetDecimal(allValues, temperatures, 5);
                WaterTemperature.Temperature = GetDecimal(allValues, temperatures, 7);
                HeatSourceInletTemperature.Temperature = GetDecimal(allValues, temperatures, 9);
                HeatSourceOutletTemperature.Temperature = GetDecimal(allValues, temperatures, 10);

                FlowRate = GetDecimal(allValues, incoming, 6);

                TotalProducedHeatEnergy = GetDecimal(allValues, heatEnergy, 0) * 1000;
                TotalProducedWaterEnergy = GetDecimal(allValues, heatEnergy, 1) * 1000;
                TotalProducedEnergy = GetDecimal(allValues, heatEnergy, 2) * 1000;

                TotalConsumedHeatEnergy = GetDecimal(allValues, powerEnergy, 0) * 1000;
                TotalConsumedWaterEnergy = GetDecimal(allValues, powerEnergy, 1) * 1000;
                TotalConsumedEnergy = GetDecimal(allValues, powerEnergy, 2) * 1000;

                RecalculatePowerConsumption();
                LastUpdated = DateTimeOffset.Now;

                _thingManager.DetectChanges(this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing XML.");
            }
        }

        private decimal? _previousTotalConsumedEnergy;
        private DateTimeOffset? _previousTotalConsumedEnergyTime;

        private void RecalculatePowerConsumption()
        {
            if (_previousTotalConsumedEnergy != null &&
                _previousTotalConsumedEnergyTime != null &&
                TotalConsumedEnergy != null)
            {
                var difference = DateTimeOffset.Now - _previousTotalConsumedEnergyTime.Value;
                if (difference.TotalHours > 2)
                {
                    PowerConsumption = 50;
                }
                else if (_previousTotalConsumedEnergy != TotalConsumedEnergy)
                {
                    PowerConsumption = Math.Round(
                        (TotalConsumedEnergy.Value - _previousTotalConsumedEnergy.Value) *
                        (60 / (decimal)difference.TotalMinutes), 0);

                    if (PowerConsumption < 50)
                    {
                        PowerConsumption = 50;
                    }
                }
            }

            if (_previousTotalConsumedEnergy == null &&
                _previousTotalConsumedEnergyTime == null)
            {
                _previousTotalConsumedEnergy = TotalConsumedEnergy;
            }
            else if (_previousTotalConsumedEnergy != TotalConsumedEnergy)
            {
                _previousTotalConsumedEnergy = TotalConsumedEnergy;
                _previousTotalConsumedEnergyTime = DateTimeOffset.Now;
            }
        }

        private decimal? GetDecimal(XElement[]? allValues, IEnumerable<XElement>? section, int index)
        {
            var id = section?.ElementAt(index).Attribute("id")?.Value;
            var element = allValues?.SingleOrDefault(v => v.Attribute("id")?.Value == id);

            if (decimal.TryParse(element?.Element("value")?.Value?.Split(' ', '°')[0], out var value))
            {
                return value;
            }

            return null;
        }

        private string? GetString(XElement[]? allValues, IEnumerable<XElement>? section, int index)
        {
            var id = section?.ElementAt(index).Attribute("id")?.Value;
            var element = allValues?.SingleOrDefault(v => v.Attribute("id")?.Value == id);
            return element?.Element("value")?.Value;
        }

        public override void Dispose()
        {
            _webSocket?.Dispose();
            _webSocket = null;
            _isRunning = false;
        }
    }
}

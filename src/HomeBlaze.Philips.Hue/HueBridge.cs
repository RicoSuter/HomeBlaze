using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Q42.HueApi;
using Q42.HueApi.Models.Bridge;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Philips.Hue
{
    [DisplayName("Philips Hue Bridge")]
    [Description("The brains of the Philips Hue smart lighting system, the Hue Bridge allows you to connect and control up to 50 lights and accessories.")]
    [ThingSetup(typeof(HueBridgeSetup))]
    public class HueBridge : PollingThing, IIconProvider, ILastUpdatedProvider,
        IConnectedDevice, IHubDevice, IPowerConsumptionSensor
    {
        private bool _isRefreshing = false;
        private readonly ILogger<HueBridge> _logger;

        public override string? Id => Bridge != null ?
            "hue.brigde." + Bridge.BridgeId :
            null;

        public override string Title => "Hue Bridge (" + (Bridge?.IpAddress ?? "?") + ")";

        public string IconName => "fab fa-hubspot";

        internal LocatedBridge? Bridge { get; set; }

        public DateTimeOffset? LastUpdated { get; private set; }

        [Configuration]
        public string? BridgeId { get; set; }

        [Configuration(IsSecret = true)]
        public string? AppKey { get; set; }

        [Configuration]
        public int RefreshInterval { get; set; } = 500;

        [State]
        public decimal? PowerConsumption => IsConnected ? 3.0m : 0;

        [State]
        public bool IsConnected { get; private set; }

        [State]
        public IThing[] Groups { get; private set; } = Array.Empty<IThing>();

        [State]
        public IThing[] Lights { get; private set; } = Array.Empty<IThing>();

        [State]
        public IThing[] Sensors { get; private set; } = Array.Empty<IThing>();

        public IEnumerable<IThing> Devices => Groups
            .Union(Lights)
            .Union(Sensors);

        protected override TimeSpan PollingInterval => TimeSpan.FromMilliseconds(RefreshInterval);

        protected override TimeSpan FailureInterval => TimeSpan.FromSeconds(5);

        public HueBridge(IThingManager thingManager, ILogger<HueBridge> logger)
            : base(thingManager, logger)
        {
            _logger = logger;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            if (_isRefreshing || string.IsNullOrEmpty(AppKey))
            {
                return;
            }

            _isRefreshing = true;
            try
            {
                if (Bridge == null)
                {
                    var bridges = await HueBridgeDiscovery.FastDiscoveryWithNetworkScanFallbackAsync(
                        TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30));

                    Bridge = bridges
                        .OrderBy(b => b.BridgeId)
                        .FirstOrDefault(b => string.IsNullOrEmpty(BridgeId) || b.BridgeId == BridgeId);

                    if (Bridge != null)
                    {
                        BridgeId = Bridge?.BridgeId;
                    }
                    else
                    {
                        _logger.LogWarning("Bridge ID {BridgeId} could not be found.", BridgeId);
                    }
                }

                if (Bridge?.IpAddress != null)
                {
                    var client = new LocalHueClient(Bridge.IpAddress);
                    client.Initialize(AppKey);

                    LastUpdated = DateTimeOffset.Now;

                    var sensorsTask = client.GetSensorsAsync();
                    var lightsTask = client.GetLightsAsync();
                    var groupsTask = client.GetGroupsAsync();

                    var sensors = await sensorsTask;
                    var sensorDevices = sensors // TODO: Use CreateOrUpdate here
                        .Select<Q42.HueApi.Models.Sensor, IThing?>(s =>
                        {
                            if (s.Type == "ZLLTemperature")
                            {
                                return Devices
                                    .OfType<HueTemperatureDevice>()
                                    .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s, sensors)
                                    ?? new HueTemperatureDevice(s, sensors, this);
                            }
                            else if (s.Type == "ZLLLightLevel")
                            {
                                return Devices
                                    .OfType<HueLightSensor>()
                                    .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s, sensors)
                                    ?? new HueLightSensor(s, sensors, this);
                            }
                            else if (s.Type == "Daylight")
                            {
                                return Devices
                                    .OfType<HueDaylightSensor>()
                                    .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s)
                                    ?? new HueDaylightSensor(s, this);
                            }
                            else if (s.Type == "ZLLPresence")
                            {
                                return Devices
                                    .OfType<HuePresenceDevice>()
                                    .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s)
                                    ?? new HuePresenceDevice(s, this);
                            }
                            else if (s.Type == "ZLLSwitch")
                            {
                                return Devices
                                    .OfType<HueDimmerSwitchDevice>()
                                    .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s)
                                    ?? new HueDimmerSwitchDevice(s, this);
                            }
                            else if (s.Type == "ZGPSwitch")
                            {
                                return Devices
                                    .OfType<HueTapSwitchDevice>()
                                    .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s)
                                    ?? new HueTapSwitchDevice(s, this);
                            }
                            else
                            {
                                return Devices
                                    .OfType<HueUnknownDevice>()
                                    .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s)
                                    ?? new HueUnknownDevice(s, this);
                            }
                        })
                        .OfType<IThing>()
                        .OrderBy(s => s.GetType().Name)
                        .ThenBy(s => s.Title)
                        .ToArray();

                    var lightDevices = (await lightsTask)
                        .Select(s => Devices
                            .OfType<HueLightDevice>()
                            .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s)
                            ?? new HueLightDevice(s, this))
                        .OrderBy(s => s.Title)
                        .ToArray();

                    var groupDevices = (await groupsTask)
                        .Select(s => Devices
                            .OfType<HueGroupDevice>()
                            .SingleOrDefault(d =>
                                d.ReferenceId == s.Id)?.Update(s, lightDevices.Where(l => s.Lights.Contains(l.ReferenceId)).ToArray())
                                ?? new HueGroupDevice(s, lightDevices.Where(l => s.Lights.Contains(l.ReferenceId)).ToArray(), this))
                        .OfType<IThing>()
                        .OrderBy(s => s.Title)
                        .ToArray();

                    Groups = groupDevices;
                    Lights = lightDevices;
                    Sensors = sensorDevices;

                    IsConnected = true;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to connect to Philips Hue hub.");
                IsConnected = false;
                throw;
            }
            finally
            {
                _isRefreshing = false;
            }
        }
    }
}

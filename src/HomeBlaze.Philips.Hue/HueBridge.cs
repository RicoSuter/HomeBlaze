using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using HueApi;
using HueApi.BridgeLocator;
using Microsoft.Extensions.Logging;
using System;
using HomeBlaze.Services.Extensions;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using HueApi.Models.Responses;
using System.Text.Json;
using HomeBlaze.Services.Json;
using HomeBlaze.Abstractions.Devices;

namespace HomeBlaze.Philips.Hue
{
    [DisplayName("Philips Hue Bridge")]
    [Description("The brains of the Philips Hue smart lighting system, the Hue Bridge allows you to connect and control up to 50 lights and accessories.")]
    [ThingSetup(typeof(HueBridgeSetup), CanEdit = true)]
    public class HueBridge : PollingThing, 
        IIconProvider, 
        ILastUpdatedProvider,
        IConnectedThing,
        IHubDevice, 
        IPowerConsumptionSensor
    {
        private bool _isRefreshing = false;
        private LocalHueApi? _client;
        private readonly ILogger<HueBridge> _logger;

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
        public IThing[] Rooms { get; private set; } = Array.Empty<IThing>();

        [State]
        public IThing[] Zones { get; private set; } = Array.Empty<IThing>();

        [State]
        public IThing[] Lights { get; private set; } = Array.Empty<IThing>();

        [State]
        public IThing[] Sensors { get; private set; } = Array.Empty<IThing>();

        public IEnumerable<IThing> Devices { get; private set; } = Array.Empty<IThing>();

        protected override TimeSpan PollingInterval => TimeSpan.FromSeconds(60);

        protected override TimeSpan FailureInterval => TimeSpan.FromSeconds(5);

        public HueBridge(IThingManager thingManager, ILogger<HueBridge> logger)
            : base(thingManager, logger)
        {
            _logger = logger;
        }

        public LocalHueApi CreateClient()
        {
            if ((AppKey) == null || (Bridge) == null)
            {
                throw new InvalidOperationException("Bridge must not be null.");
            }

            var client = new LocalHueApi(Bridge.IpAddress, AppKey);
            return client;
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
                    if (_client is null)
                    {
                        _client = new LocalHueApi(Bridge.IpAddress, AppKey);
                        _client.OnEventStreamMessage += OnEventStreamMessage;
                        _client.StartEventStream();
                    }

                    LastUpdated = DateTimeOffset.Now;

                    var devices = await _client.GetDevicesAsync();
                    var buttons = await _client.GetButtonsAsync();
                    var zigbeeConnectivities = await _client.GetZigbeeConnectivityAsync();
                    var lights = await _client.GetLightsAsync();
                    var groupedLights = await _client.GetGroupedLightsAsync();
                    var rooms = await _client.GetRoomsAsync();
                    var motions = await _client.GetMotionsAsync();
                    var zones = await _client.GetZonesAsync();

                    var allDevices = devices
                        .Data
                        .CreateOrUpdate(Devices,
                            (a, b) => a.Id == b.DeviceId,
                            (a, device) =>
                            {
                                var services = device.Services?.Select(s => s.Rid).ToArray() ?? Array.Empty<Guid>();
                                var zigbeeConnectivity = zigbeeConnectivities.Data.SingleOrDefault(s => services.Contains(s.Id));

                                var lightService = device.Services?.SingleOrDefault(s => s.Rtype == "light");
                                if (lightService is not null)
                                {
                                    var light = lights.Data.SingleOrDefault(l => l.Id == lightService.Rid);
                                    if (light is not null)
                                    {
                                        ((HueLightDevice)a).Update(device, zigbeeConnectivity, light);
                                        return;
                                    }
                                }

                                var motionService = device.Services?.SingleOrDefault(s => s.Rtype == "motion");
                                if (motionService is not null)
                                {
                                    var motion = motions.Data.SingleOrDefault(l => l.Id == motionService.Rid);
                                    if (motion is not null)
                                    {
                                        ((HuePresenceDevice)a).Update(device, zigbeeConnectivity, motion);
                                        return;
                                    }
                                }

                                var buttonsService = device.Services?
                                   .Where(s => s.Rtype == "button")
                                   .Select(s => buttons.Data.SingleOrDefault(b => b.Id == s.Rid))
                                   .Where(s => s is not null)
                                   .ToArray();

                                if (buttonsService is not null && buttonsService.Any())
                                {
                                    ((HueButtonDevice)a).Update(device, zigbeeConnectivity, buttonsService!);
                                    return;
                                }

                                a.Update(device, zigbeeConnectivity);
                            },
                            device =>
                            {
                                var services = device.Services?.Select(s => s.Rid).ToArray() ?? Array.Empty<Guid>();
                                var zigbeeConnectivity = zigbeeConnectivities.Data.SingleOrDefault(s => services.Contains(s.Id));

                                var lightService = device.Services?.SingleOrDefault(s => s.Rtype == "light");
                                if (lightService is not null)
                                {
                                    var light = lights.Data.SingleOrDefault(l => l.Id == lightService.Rid);
                                    if (light is not null)
                                    {
                                        return new HueLightDevice(device, zigbeeConnectivity, light, this);
                                    }
                                }

                                var motionService = device.Services?.SingleOrDefault(s => s.Rtype == "motion");
                                if (motionService is not null)
                                {
                                    var motion = motions.Data.SingleOrDefault(l => l.Id == motionService.Rid);
                                    if (motion is not null)
                                    {
                                        return new HuePresenceDevice(device, zigbeeConnectivity, motion, this);
                                    }
                                }

                                var buttonsService = device.Services?
                                   .Where(s => s.Rtype == "button")
                                   .Select(s => buttons.Data.SingleOrDefault(b => b.Id == s.Rid))
                                   .Where(s => s is not null)
                                   .ToArray();

                                if (buttonsService is not null && buttonsService.Any())
                                {
                                    return new HueButtonDevice(device, zigbeeConnectivity, buttonsService!, this);
                                }

                                return new HueDevice(device, zigbeeConnectivity, this);
                            })
                        .OrderBy(c => c.Title)
                        .ToArray();

                    Lights = Devices
                        .OfType<HueLightDevice>()
                        .ToArray();

                    Sensors = Devices
                        .Except(Lights)
                        .ToArray();

                    Rooms = rooms
                        .Data
                        .CreateOrUpdate(Rooms,
                            (a, b) => a.Id == b.ResourceId,
                            (a, room) =>
                            {
                                var services = room.Services?.Select(s => s.Rid).ToArray() ?? Array.Empty<Guid>();
                                var groupedLight = groupedLights.Data.SingleOrDefault(g => services.Contains(g.Id));
                                a.Update(room, groupedLight, Devices
                                    .OfType<HueLightDevice>()
                                    .Where(l => room.Children.Any(c => c.Rid == l.DeviceId || c.Rid == l.LightResource.Id))
                                    .ToArray());
                            },
                            room =>
                            {
                                var services = room.Services?.Select(s => s.Rid).ToArray() ?? Array.Empty<Guid>();
                                var groupedLight = groupedLights.Data.SingleOrDefault(g => services.Contains(g.Id));
                                return new HueGroupDevice(room, groupedLight, Devices
                                    .OfType<HueLightDevice>()
                                    .Where(l => room.Children.Any(c => c.Rid == l.DeviceId || c.Rid == l.LightResource.Id))
                                    .ToArray(), this);
                            })
                        .OrderBy(c => c.Title)
                        .ToArray();

                    Zones = zones
                       .Data
                       .CreateOrUpdate(Zones,
                           (a, b) => a.Id == b.ResourceId,
                           (a, zone) =>
                           {
                               var services = zone.Services?.Select(s => s.Rid).ToArray() ?? Array.Empty<Guid>();
                               var groupedLight = groupedLights.Data.SingleOrDefault(g => services.Contains(g.Id));
                               a.Update(zone, groupedLight, Devices
                                   .OfType<HueLightDevice>()
                                   .Where(l => zone.Children.Any(c => c.Rid == l.DeviceId || c.Rid == l.LightResource.Id))
                                   .ToArray());
                           },
                           zone =>
                           {
                               var services = zone.Services?.Select(s => s.Rid).ToArray() ?? Array.Empty<Guid>();
                               var groupedLight = groupedLights.Data.SingleOrDefault(g => services.Contains(g.Id));
                               return new HueGroupDevice(zone, groupedLight, Devices
                                   .OfType<HueLightDevice>()
                                   .Where(l => zone.Children.Any(c => c.Rid == l.DeviceId || c.Rid == l.LightResource.Id))
                                   .ToArray(), this);
                           })
                       .OrderBy(c => c.Title)
                       .ToArray();

                    Devices = allDevices
                        .OfType<IThing>()
                        .ToArray();

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

        private void OnEventStreamMessage(string bridgeIp, List<EventStreamResponse> events)
        {
            foreach (var ev in events)
            {
                foreach (var data in ev.Data)
                {
                    if (data.Type == "button")
                    {
                        var buttonDevice = Devices.OfType<HueDevice>().SingleOrDefault(d => d.DeviceId == data.Owner?.Rid) as HueButtonDevice;
                        var button = buttonDevice?.Buttons.SingleOrDefault(b => b.ReferenceId == data.Id);
                        if (button is not null && buttonDevice is not null)
                        {
                            JsonUtilities.PopulateObject(button.ButtonResource, JsonSerializer.Serialize(data));
                            ThingManager.DetectChanges(buttonDevice);
                        }
                    }
                    else if (data.Type == "light")
                    {
                        var lightDevice = Devices.OfType<HueDevice>().SingleOrDefault(d => d.DeviceId == data.Owner?.Rid) as HueLightDevice;
                        if (lightDevice is not null)
                        {
                            JsonUtilities.PopulateObject(lightDevice.LightResource, JsonSerializer.Serialize(data));
                            ThingManager.DetectChanges(lightDevice);

                            foreach (var room in Rooms.OfType<HueGroupDevice>().Where(r => r.Lights.Contains(lightDevice)))
                            {
                                ThingManager.DetectChanges(room);
                            }

                            foreach (var zone in Zones.OfType<HueGroupDevice>().Where(r => r.Lights.Contains(lightDevice)))
                            {
                                ThingManager.DetectChanges(zone);
                            }
                        }
                    }
                    else if (data.Type == "grouped_light")
                    {
                        var group = Rooms
                            .Union(Zones)
                            .OfType<HueGroupDevice>()
                            .SingleOrDefault(d => d.GroupedLight?.Id == data.Id);

                        if (group is not null)
                        {
                            JsonUtilities.PopulateObject(group.GroupedLight!, JsonSerializer.Serialize(data));
                            ThingManager.DetectChanges(group);
                        }
                    }
                    else if (data.Type == "motion")
                    {


                    }
                    else if (data.Type == "temperature")
                    {

                    }
                    else if (data.Type == "light_level")
                    {


                    }
                    else
                    {

                    }
                }
            }
        }
    }
}

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Inputs;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using HomeBlaze.Services.Extensions;
using HueApi;
using HueApi.BridgeLocator;
using HueApi.Models.Responses;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly IEventManager _eventManager;
        private readonly ILogger<HueBridge> _logger;

        internal LocatedBridge? Bridge { get; set; }

        public override string Title => "Hue Bridge (" + (Bridge?.IpAddress ?? "?") + ")";

        public string IconName => "fab fa-hubspot";

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

        public HueBridge(IThingManager thingManager, IEventManager eventManager, ILogger<HueBridge> logger)
            : base(thingManager, logger)
        {
            _eventManager = eventManager;
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

                    var zigbeeConnectivities = await _client.GetZigbeeConnectivityAsync();
                    var devicePowers = await _client.GetDevicePowersAsync();

                    var devices = await _client.GetDevicesAsync();

                    var rooms = await _client.GetRoomsAsync();
                    var zones = await _client.GetZonesAsync();

                    var lights = await _client.GetLightsAsync();
                    var buttons = await _client.GetButtonsAsync();
                    var motions = await _client.GetMotionsAsync();
                    var groupedLights = await _client.GetGroupedLightsAsync();
                    var temperatures = await _client.GetTemperaturesAsync();
                    var lightLevels = await _client.GetLightLevelsAsync();

                    var allDevices = devices
                        .Data
                        .CreateOrUpdate(Devices,
                            (a, b) => a.Id == b.ResourceId,
                            (a, device) =>
                            {
                                var services = device.Services?.Select(s => s.Rid).ToArray() ?? Array.Empty<Guid>();
                                var zigbeeConnectivity = zigbeeConnectivities.Data.SingleOrDefault(s => services.Contains(s.Id));
                                var devicePower = devicePowers.Data.SingleOrDefault(s => services.Contains(s.Id));

                                var lightService = device.Services?.SingleOrDefault(s => s.Rtype == "light");
                                if (lightService is not null)
                                {
                                    var light = lights.Data.SingleOrDefault(l => l.Id == lightService.Rid);
                                    if (light is not null)
                                    {
                                        ((HueLightbulb)a).Update(device, zigbeeConnectivity, light);
                                        return;
                                    }
                                }

                                var motionService = device.Services?.SingleOrDefault(s => s.Rtype == "motion");
                                if (motionService is not null)
                                {
                                    var motion = motions.Data.SingleOrDefault(l => l.Id == motionService.Rid);
                                    if (motion is not null && a is HueMotionDevice motionDevice)
                                    {
                                        var temperature = temperatures.Data.SingleOrDefault(s => services.Contains(s.Id));
                                        var lightLevel = lightLevels.Data.SingleOrDefault(s => services.Contains(s.Id));
                                        ((HueMotionDevice)a).Update(device, zigbeeConnectivity, devicePower, temperature, lightLevel, motion);
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
                                var devicePower = devicePowers.Data.SingleOrDefault(s => services.Contains(s.Id));

                                var lightService = device.Services?.SingleOrDefault(s => s.Rtype == "light");
                                if (lightService is not null)
                                {
                                    var light = lights.Data.SingleOrDefault(l => l.Id == lightService.Rid);
                                    if (light is not null)
                                    {
                                        return new HueLightbulb(device, zigbeeConnectivity, light, this);
                                    }
                                }

                                var motionService = device.Services?.SingleOrDefault(s => s.Rtype == "motion");
                                if (motionService is not null)
                                {
                                    var motion = motions.Data.SingleOrDefault(l => l.Id == motionService.Rid);
                                    if (motion is not null)
                                    {
                                        var temperature = temperatures.Data.SingleOrDefault(s => services.Contains(s.Id));
                                        var lightLevel = lightLevels.Data.SingleOrDefault(s => services.Contains(s.Id));

                                        return new HueMotionDevice(device, zigbeeConnectivity, devicePower, temperature, lightLevel, motion, this);
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

                    Lights = allDevices
                        .OfType<HueLightbulb>()
                        .ToArray();

                    Sensors = allDevices
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
                                a.Update(room, groupedLight, allDevices
                                    .OfType<HueLightbulb>()
                                    .Where(l => room.Children.Any(c => c.Rid == l.ResourceId || c.Rid == l.LightResource.Id))
                                    .ToArray());
                            },
                            room =>
                            {
                                var services = room.Services?.Select(s => s.Rid).ToArray() ?? Array.Empty<Guid>();
                                var groupedLight = groupedLights.Data.SingleOrDefault(g => services.Contains(g.Id));
                                return new HueGroup(room, groupedLight, allDevices
                                    .OfType<HueLightbulb>()
                                    .Where(l => room.Children.Any(c => c.Rid == l.ResourceId || c.Rid == l.LightResource.Id))
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
                                a.Update(zone, groupedLight, allDevices
                                    .OfType<HueLightbulb>()
                                    .Where(l => zone.Children.Any(c => c.Rid == l.ResourceId || c.Rid == l.LightResource.Id))
                                    .ToArray());
                            },
                            zone =>
                            {
                                var services = zone.Services?.Select(s => s.Rid).ToArray() ?? Array.Empty<Guid>();
                                var groupedLight = groupedLights.Data.SingleOrDefault(g => services.Contains(g.Id));
                                return new HueGroup(zone, groupedLight, allDevices
                                    .OfType<HueLightbulb>()
                                    .Where(l => zone.Children.Any(c => c.Rid == l.ResourceId || c.Rid == l.LightResource.Id))
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
                        var buttonDevice = Devices
                            .OfType<HueButtonDevice>()
                            .SingleOrDefault(d => d.ResourceId == data.Owner?.Rid);

                        var button = buttonDevice?.Buttons.SingleOrDefault(b => b.ResourceId == data.Id);
                        if (button is not null && buttonDevice is not null)
                        {
                            DateTimeOffset? buttonChangeDate = data.ExtensionData.TryGetValue("button", out var buttonValue) ?
                                buttonValue
                                    .GetProperty("button_report")
                                    .GetProperty("updated")
                                    .GetDateTimeOffset() : null;

                            if (buttonChangeDate.HasValue)
                            {
                                button.ButtonChangeDate = buttonChangeDate.Value;
                            }

                            button.Update(Merge(button.ButtonResource, data));
                            button.LastUpdated = DateTimeOffset.Now;

                            if (button.ButtonResource.Button?.LastEvent.HasValue == true)
                            {
                                _eventManager.Publish(new ButtonEvent
                                {
                                    ThingId = buttonDevice.Id,
                                    ButtonState = HueButton.GetButtonState(button.ButtonResource.Button.LastEvent.Value)
                                });
                            }

                            ThingManager.DetectChanges(buttonDevice);
                        }
                    }
                    else if (data.Type == "light")
                    {
                        var lightDevice = Devices
                            .OfType<HueLightbulb>()
                            .SingleOrDefault(d => d.ResourceId == data.Owner?.Rid);

                        if (lightDevice is not null)
                        {
                            lightDevice.LightResource = Merge(lightDevice.LightResource, data);
                            lightDevice.LastUpdated = DateTimeOffset.Now;
                          
                            ThingManager.DetectChanges(lightDevice);

                            foreach (var room in Rooms.OfType<HueGroup>().Where(r => r.Lights.Contains(lightDevice)))
                            {
                                ThingManager.DetectChanges(room);
                            }

                            foreach (var zone in Zones.OfType<HueGroup>().Where(r => r.Lights.Contains(lightDevice)))
                            {
                                ThingManager.DetectChanges(zone);
                            }
                        }
                    }
                    else if (data.Type == "grouped_light")
                    {
                        var group = Rooms
                            .Union(Zones)
                            .OfType<HueGroup>()
                            .SingleOrDefault(d => d.GroupedLight?.Id == data.Id);

                        if (group is not null && group.GroupedLight is not null)
                        {
                            group.GroupedLight = Merge(group.GroupedLight, data);
                            group.LastUpdated = DateTimeOffset.Now;
                            ThingManager.DetectChanges(group);
                        }
                    }
                    else if (data.Type == "motion")
                    {
                        var motion = Devices
                            .OfType<HueMotionDevice>()
                            .SingleOrDefault(d => d.MotionResource.Id == data.Id);

                        if (motion is not null && motion.MotionResource is not null)
                        {
                            motion.MotionResource = Merge(motion.MotionResource, data);
                            motion.LastUpdated = DateTimeOffset.Now;
                            ThingManager.DetectChanges(motion);
                        }
                    }
                    else if (data.Type == "temperature")
                    {
                        var motion = Devices
                            .OfType<HueMotionDevice>()
                            .SingleOrDefault(d => d.TemperatureResource?.Id == data.Id);

                        if (motion is not null && motion.MotionResource is not null)
                        {
                            motion.TemperatureResource = Merge(motion.TemperatureResource, data);
                            motion.LastUpdated = DateTimeOffset.Now;
                            ThingManager.DetectChanges(motion);
                        }
                    }
                    else if (data.Type == "light_level")
                    {
                        var motion = Devices
                           .OfType<HueMotionDevice>()
                           .SingleOrDefault(d => d.LightLevelResource?.Id == data.Id);

                        if (motion is not null && motion.MotionResource is not null)
                        {
                            motion.LightLevelResource = Merge(motion.LightLevelResource, data);
                            motion.LastUpdated = DateTimeOffset.Now;
                            ThingManager.DetectChanges(motion);
                        }
                    }
                    else if (data.Type == "device_power")
                    {
                        var motion = Devices
                            .OfType<HueMotionDevice>()
                            .SingleOrDefault(d => d.DevicePowerResource?.Id == data.Id);

                        if (motion is not null && motion.MotionResource is not null)
                        {
                            motion.DevicePowerResource = Merge(motion.DevicePowerResource, data);
                            motion.LastUpdated = DateTimeOffset.Now;
                            ThingManager.DetectChanges(motion);
                        }
                    }
                    else if (data.Type == "scene")
                    {

                    }
                    else
                    {

                    }
                }
            }
        }

        private static T Merge<T>(T currentResource, EventStreamData newPartialResource)
        {
            var o1 = JObject.Parse(JsonSerializer.Serialize(currentResource));
            var o2 = JObject.Parse(JsonSerializer.Serialize(newPartialResource));

            o1.Merge(o2, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });

            return JsonSerializer.Deserialize<T>(o1.ToString())!;
        }
    }
}

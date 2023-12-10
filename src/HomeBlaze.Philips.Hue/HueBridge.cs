using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using HueApi;
using HueApi.BridgeLocator;
using HueApi.Models;
using Microsoft.Extensions.Logging;
using MudBlazor.Extensions;
using System;
using System.Collections.Generic;
using HomeBlaze.Services.Extensions;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Philips.Hue
{
    [DisplayName("Philips Hue Bridge")]
    [Description("The brains of the Philips Hue smart lighting system, the Hue Bridge allows you to connect and control up to 50 lights and accessories.")]
    [ThingSetup(typeof(HueBridgeSetup), CanEdit = true)]
    public class HueBridge : PollingThing, IIconProvider, ILastUpdatedProvider,
        IConnectedThing,
        //IHubDevice, 
        IPowerConsumptionSensor
    {
        private bool _isRefreshing = false;
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

        [State]
        public HueDevice[] Devices { get; private set; } = Array.Empty<HueDevice>();

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
                    var client = new LocalHueApi(Bridge.IpAddress, AppKey);

                    LastUpdated = DateTimeOffset.Now;

                    //var resourcesTask = client.GetResourcesAsync();
                    //var sensorsTask = client.GetDevicesAsync();
                    //var lightsTask = client.GetLightsAsync();
                    //var roomsTask = client.GetRoomsAsync();
                    //var zonesTask = client.GetZonesAsync();

                    //var resources = await resourcesTask;
                    //var sensors = await sensorsTask;
                    //var lights = await lightsTask;
                    //var rooms = await roomsTask;
                    //var zones = await zonesTask;

                    var resources = await client.GetResourcesAsync();

                    var devices = await client.GetDevicesAsync();
                    var zigbeeConnectivities = await client.GetZigbeeConnectivityAsync();
                    var lights = await client.GetLightsAsync();
                    var rooms = await client.GetRoomsAsync();
                    var zones = await client.GetZonesAsync();

                    Devices = devices
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

                                return new HueDevice(device, zigbeeConnectivity, this);
                            })
                        .OrderBy(c => c.Title)
                        .ToArray();

                    //var sensors = await sensorsTask;
                    //var sensorDevices = sensors // TODO: Use CreateOrUpdate here
                    //    .Data
                    //    .Select<Device, IThing>(s =>
                    //    {
                    //        var motion = s.Services?.SingleOrDefault(s => s.Rtype == "motion");
                    //        if (motion is not null)
                    //        {
                    //            //var x = client.GetEntertainmentServiceAsync(motion.Rid).GetAwaiter().GetResult();
                    //        }

                    //        if (s.Type == "ZLLTemperature")
                    //        {
                    //            return Devices
                    //                .OfType<HueTemperatureDevice>()
                    //                .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s, sensors.Data)
                    //                ?? new HueTemperatureDevice(s, sensors.Data, this);
                    //        }
                    //        else if (s.Type == "ZLLLightLevel")
                    //        {
                    //            return Devices
                    //                .OfType<HueLightSensor>()
                    //                .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s, sensors.Data)
                    //                ?? new HueLightSensor(s, sensors.Data, this);
                    //        }
                    //        else if (s.Type == "Daylight")
                    //        {
                    //            return Devices
                    //                .OfType<HueDaylightSensor>()
                    //                .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s)
                    //                ?? new HueDaylightSensor(s, this);
                    //        }
                    //        else if (s.Type == "ZLLPresence")
                    //        {
                    //            return Devices
                    //                .OfType<HuePresenceDevice>()
                    //                .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s)
                    //                ?? new HuePresenceDevice(s, this);
                    //        }
                    //        else if (s.Type == "ZLLSwitch")
                    //        {
                    //            return Devices
                    //                .OfType<HueDimmerSwitchDevice>()
                    //                .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s)
                    //                ?? new HueDimmerSwitchDevice(s, this);
                    //        }
                    //        else if (s.Type == "ZGPSwitch")
                    //        {
                    //            return Devices
                    //                .OfType<HueTapSwitchDevice>()
                    //                .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s)
                    //                ?? new HueTapSwitchDevice(s, this);
                    //        }
                    //        else
                    //        {
                    //            return Devices
                    //                .OfType<HueDeviceBase>()
                    //                .SingleOrDefault(d => d.ResourceId == s.Id)?.Update(s)
                    //                ?? new HueDeviceBase(s, this);
                    //        }
                    //    })
                    //    .OfType<IThing>()
                    //    .OrderBy(s => s.GetType().Name)
                    //    .ThenBy(s => s.Title)
                    //    .ToArray();

                    //var lights = await lightsTask;
                    //var lightDevices = lights
                    //    .Data
                    //    .Select(s => Devices
                    //        .OfType<HueLightDevice>()
                    //        .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s)
                    //        ?? new HueLightDevice(s, this))
                    //    .OrderBy(s => s.Title)
                    //    .ToArray();

                    //var roomDevices = (await roomsTask)
                    //    .Data
                    //    .Select(s => Devices
                    //        .OfType<HueGroupDevice>()
                    //        .SingleOrDefault(d =>
                    //            d.ReferenceId == s.Id)?.Update(s, lightDevices.Where(l => s.Children.Any(c => c.Rid == l.ReferenceId)).ToArray())
                    //            ?? new HueGroupDevice(s, lightDevices.Where(l => s.Children.Any(c => c.Rid == l.ReferenceId)).ToArray(), this))
                    //    .OfType<IThing>()
                    //    .OrderBy(s => s.Title)
                    //    .ToArray();

                    //var zoneDevices = (await zonesTask)
                    //   .Data
                    //   .Select(s => Devices
                    //       .OfType<HueGroupDevice>()
                    //       .SingleOrDefault(d =>
                    //           d.ReferenceId == s.Id)?.Update(s, lightDevices.Where(l => s.Children.Any(c => c.Rid == l.ReferenceId)).ToArray())
                    //           ?? new HueGroupDevice(s, lightDevices.Where(l => s.Children.Any(c => c.Rid == l.ReferenceId)).ToArray(), this))
                    //   .OfType<IThing>()
                    //   .OrderBy(s => s.Title)
                    //   .ToArray();


                    var l1 = lights.Data.SingleOrDefault(s => s.Metadata?.Name == "Ruheraum 1");
                    var b1 = devices.Data.SingleOrDefault(s => s.Metadata?.Name == "Ruheraum 1");
                    var r1 = resources.Data.SingleOrDefault(s => s.Id == b1!.Services!.Single(s => s.Rtype == "zigbee_connectivity").Rid);

                    //Rooms = roomDevices;
                    //Zones = zoneDevices;
                    Lights = Devices.OfType<HueLightDevice>().ToArray();
                    //Sensors = sensorDevices;

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

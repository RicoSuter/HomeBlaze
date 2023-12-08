using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Rssdp;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Sonos.Base;

namespace HomeBlaze.Sonos
{
    [DisplayName("Sonos System")]
    [ThingSetup(typeof(SonosSetup))]
    public class SonosSystem : PollingThing, IIconProvider, ILastUpdatedProvider, IVirtualThing
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public bool IsRefreshing { get; private set; }

        public override string Title => "Sonos System";

        public string IconName => "fa-solid fa-music";

        public DateTimeOffset? LastUpdated { get; private set; }

        [State]
        public SonosDevice[] Devices { get; private set; } = Array.Empty<SonosDevice>();

        public SonosSystem(
            IThingManager thingManager,
            IHttpClientFactory httpClientFactory,
            ILogger<SonosSystem> logger)
            : base(thingManager, logger)
        {
            _httpClientFactory = httpClientFactory;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            if (!IsRefreshing)
            {
                IsRefreshing = true;
                try
                {
                    using var httpClient = _httpClientFactory.CreateClient();

                    var foundDevices = await FindSonosDevicesAsync(httpClient);

                    Devices =
                        (await Task.WhenAll(foundDevices.Values.Select(rootDevice => Task.Run(async () =>
                        {
                            try
                            {
                                var sonosDevice = new global::Sonos.Base.SonosDevice(
                                    new SonosDeviceOptions(rootDevice.UrlBase, new SonosServiceProvider()));

                                var device = Devices.FirstOrDefault(d => d.Uuid == rootDevice.Uuid);
                                if (device != null)
                                {
                                    device.Update(rootDevice, sonosDevice);
                                }
                                else
                                {
                                    device = new SonosDevice(this, rootDevice, sonosDevice);
                                }

                                await device.RefreshAsync();
                                return device;
                            }
                            catch
                            {
                                return null;
                            }
                        }))))
                        .Where(d => d is not null)
                        .OrderBy(d => d!.ModelName)
                        .ToArray()!;

                    LastUpdated = DateTime.Now;
                }
                finally
                {
                    IsRefreshing = false;
                }
            }
        }

        private static async Task<ConcurrentDictionary<string, SsdpRootDevice>> FindSonosDevicesAsync(HttpClient httpClient)
        {
            var foundDevices = new ConcurrentDictionary<string, SsdpRootDevice>();

            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var searchTasks = interfaces
                .Where(i => i.OperationalStatus != OperationalStatus.Down)
                .SelectMany(i => i.GetIPProperties().UnicastAddresses.Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork))
                .Where(a => a is not null)
                .Select(a => Task.Run(async () =>
                {
                    var deviceLocator = new SsdpDeviceLocator(a.Address.ToString());
                    var devices = await deviceLocator.SearchAsync("urn:schemas-upnp-org:device:ZonePlayer:1", TimeSpan.FromSeconds(5));
                    foreach (var device in devices)
                    {
                        var deviceInfo = await device.GetDeviceInfo(httpClient);
                        if (deviceInfo.Manufacturer.Contains("sonos", StringComparison.OrdinalIgnoreCase))
                        {
                            if (foundDevices.ContainsKey(deviceInfo.Uuid) == false)
                            {
                                foundDevices[deviceInfo.Uuid] = deviceInfo;
                            }
                        }
                    }
                }));

            await Task.WhenAll(searchTasks);

            return foundDevices;
        }
    }
}

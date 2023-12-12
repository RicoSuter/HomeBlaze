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
using Sonos.Base.Events.Http;
using Microsoft.Extensions.Options;

namespace HomeBlaze.Sonos
{
    [DisplayName("Sonos System")]
    [ThingSetup(typeof(SonosSetup), CanEdit = true)]
    public class SonosSystem :
        PollingThing,
        IIconProvider,
        ILastUpdatedProvider,
        IVirtualThing
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private SonosEventReceiver? _sonosEventBus;

        public bool IsRefreshing { get; private set; }

        public override string Title => "Sonos System";

        public string IconName => "fa-solid fa-music";

        public DateTimeOffset? LastUpdated { get; private set; }

        [Configuration]
        public string? Host { get; set; }

        [State]
        public SonosDevice[] Devices { get; private set; } = Array.Empty<SonosDevice>();

        protected override TimeSpan PollingInterval => TimeSpan.FromMinutes(3);

        public SonosSystem(
            IThingManager thingManager,
            IHttpClientFactory httpClientFactory,
            ILogger<SonosSystem> logger)
            : base(thingManager, logger)
        {
            _httpClientFactory = httpClientFactory;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _sonosEventBus = new SonosEventReceiver(
                _httpClientFactory.CreateClient(),
                null,
                null,
                Options.Create(new SonosEventReceiverOptions
                {
                    Host = Host
                }));

            await _sonosEventBus.StartAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            await _sonosEventBus!.StopAsync(cancellationToken);
        }

        [Operation]
        public async Task RefreshAsync(CancellationToken cancellationToken)
        {
            await PollAsync(cancellationToken);
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
                    var currentDevices = Devices;

                    var devices = await Task.WhenAll(foundDevices.Values
                        .Select(rootDevice => Task.Run(async () =>
                        {
                            try
                            {
                                var device = currentDevices.FirstOrDefault(d => d.Uuid == rootDevice.Uuid);
                                if (device == null)
                                {
                                    var sonosDevice = new global::Sonos.Base.SonosDevice(
                                        new SonosDeviceOptions(rootDevice.UrlBase,
                                        new SonosServiceProvider(_httpClientFactory, null, _sonosEventBus)));

                                    device = new SonosDevice(this, rootDevice, sonosDevice);
                                    await device.InitializeAsync();
                                }

                                await device.RefreshAsync();
                                return device;
                            }
                            catch
                            {
                                return null;
                            }
                        })));

                    Devices = devices
                        .Where(d => d is not null)
                        .OrderBy(d => d!.ModelName)
                        .ToArray()!;

                    foreach (var removedDevice in currentDevices.Except(Devices))
                    {
                        await removedDevice.DisposeAsync();
                    }

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

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Namotion.Devices.Abstractions.Utilities;
using Namotion.Proxy;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Namotion.Shelly
{
    [ThingType("HomeBlaze.Shelly.ShellyDevice")]
    [DisplayName("Shelly Device")]
    [GenerateProxy]
    public abstract class ShellyDeviceBase :
        PollingThing,
        ILastUpdatedProvider,
        IIconProvider,
        IConnectedThing,
        INetworkAdapter
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public override string Title => $"Shelly: {Information?.Name ?? Information?.Application}";

        public virtual DateTimeOffset? LastUpdated { get; protected set; }

        public virtual string IconName => "fas fa-box";

        [Configuration]
        public virtual string? IpAddress { get; set; }

        [Configuration]
        public virtual int RefreshInterval { get; set; } = 15 * 1000;

        [State]
        public virtual bool IsConnected { get; protected set; }

        [ScanForState]
        public virtual ShellyInformation? Information { get; protected set; }

        [State]
        public virtual ShellyEnergyMeter? EnergyMeter { get; protected set; }

        [State]
        public virtual ShellyCover? Cover { get; protected set; }

        protected override TimeSpan PollingInterval =>
            TimeSpan.FromMilliseconds(Cover?.IsMoving == true ? 1000 : RefreshInterval);

        public static ShellyDevice Create(Action<ShellyDevice>? configure = null)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddShellyDevice(string.Empty, configure);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider.GetRequiredKeyedService<ShellyDevice>(string.Empty);
        }

        public ShellyDeviceBase(IHttpClientFactory httpClientFactory, ILogger<ShellyDevice> logger, IThingManager? thingManager = null)
            : base(thingManager!, logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();

                if (Information == null)
                {
                    var infoResponse = await httpClient.GetAsync($"http://{IpAddress}/shelly", cancellationToken);
                    var json = await infoResponse.Content.ReadAsStringAsync(cancellationToken);
                    Information = JsonSerializer.Deserialize<ShellyInformation>(json);
                }

                await RefreshAsync(cancellationToken);
            }
            catch
            {
                IsConnected = false;
                throw;
            }
        }

        public void Reset()
        {
            Information = null;
            Cover = null;
        }

        internal async Task CallHttpGetAsync(string route, CancellationToken cancellationToken)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient();

                await httpClient.GetAsync($"http://{IpAddress}/" + route, cancellationToken);
                await Task.Delay(250);
                await RefreshAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to call HTTP GET on Shelly device.");
            }
        }

        internal async Task RefreshAsync(CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            if (Information?.Profile == "cover")
            {
                var coverResponse = await httpClient.GetAsync($"http://{IpAddress}/roller/0", cancellationToken);
                var json = await coverResponse.Content.ReadAsStringAsync(cancellationToken);
                Cover = JsonUtilities.PopulateOrDeserialize(Cover, json);
            }
            else if (Information?.Profile == "triphase")
            {
                var coverResponse = await httpClient.GetAsync($"http://{IpAddress}/rpc/EM.GetStatus?id=0", cancellationToken);
                var json = await coverResponse.Content.ReadAsStringAsync(cancellationToken);
                EnergyMeter = JsonUtilities.PopulateOrDeserialize(EnergyMeter, json);
            }

            LastUpdated = DateTimeOffset.Now;
            IsConnected = true;

            ThingManager.DetectChanges(this);
        }
    }
}

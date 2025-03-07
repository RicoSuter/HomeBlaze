using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Services.Abstractions;

using Namotion.Devices.Abstractions.Utilities;
using Namotion.Interceptor.Attributes;

namespace Namotion.Shelly
{
    [ThingType("HomeBlaze.Shelly.ShellyDevice")]
    [DisplayName("Shelly Device")]
    [InterceptorSubject]
    public partial class ShellyDevice :
        PollingThing,
        IConnectedThing,
        INetworkAdapter,
        IIconProvider,
        ILastUpdatedProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public override string Title => $"Shelly: {Information?.Name ?? Information?.Application}";

        public partial DateTimeOffset? LastUpdated { get; protected set; }

        public string IconName => "fas fa-box";

        [Configuration]
        public string? IpAddress { get; set; }

        [Configuration]
        public int RefreshInterval { get; set; } = 15 * 1000;

        [State]
        public partial bool IsConnected { get; internal set; }

        [ScanForState]
        public partial ShellyInformation? Information { get; protected set; }

        [State]
        public partial ShellyEnergyMeter? EnergyMeter { get; internal set; }

        [State]
        public partial ShellySwitch? Switch0 { get; internal set; }

        [State]
        public partial ShellySwitch? Switch1 { get; internal set; }

        [State]
        public virtual ShellyCover? Cover { get; protected set; }

        protected override TimeSpan PollingInterval =>
            TimeSpan.FromMilliseconds(Cover?.IsMoving == true ? 1000 : RefreshInterval);

        internal ShellyWebSocketClient? WebSocketClient { get; private set; }

        public ShellyDevice(IHttpClientFactory httpClientFactory, ILogger<ShellyDevice> logger) 
            : base( logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (WebSocketClient == null)
                {
                    WebSocketClient = new ShellyWebSocketClient(this, _logger);
                }

                WebSocketClient?.StartWebSocket(cancellationToken);

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

        public override void Reset()
        {
            WebSocketClient?.Dispose();
            WebSocketClient = null;

            Information = null;
            Cover = null;
            Switch0 = null;
            Switch1 = null;
            EnergyMeter = null;

            base.Reset();
        }

        public override void Dispose()
        {
            Reset();
            base.Dispose();
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
                var emStatusResponse = await httpClient.GetAsync($"http://{IpAddress}/rpc/EM.GetStatus?id=0", cancellationToken);
                var json = await emStatusResponse.Content.ReadAsStringAsync(cancellationToken);
                EnergyMeter = JsonUtilities.PopulateOrDeserialize(EnergyMeter, json);
               
                if (EnergyMeter is not null)
                {
                    var emDataStatusResponse = await httpClient.GetAsync($"http://{IpAddress}/rpc/EMData.GetStatus?id=0", cancellationToken);
                    json = await emDataStatusResponse.Content.ReadAsStringAsync(cancellationToken);
                    EnergyMeter.EnergyData = JsonUtilities.PopulateOrDeserialize(EnergyMeter.EnergyData, json);
                    EnergyMeter.Update();
                }
            }

            LastUpdated = DateTimeOffset.Now;
            IsConnected = true;
        }
    }
}

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Nuki.Model;
using HomeBlaze.Services;
using HomeBlaze.Services.Things;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Nuki
{
    // Docs: https://developer.nuki.io/page/nuki-bridge-http-api-1-12/4#heading--doorsensor-states

    [DisplayName("Nuki Bridge")]
    [ThingSetup(typeof(NukiBridgeSetup))]
    public class NukiBridge : PollingThing, IIconProvider,
        IConnectedDevice, IHubDevice, IPowerConsumptionSensor
    {
        internal readonly IThingManager _thingManager;
        internal readonly IHttpClientFactory _httpClientFactory;
        internal readonly ILogger<NukiBridge> _logger;

        public BridgeInformation? Information { get; private set; }

        public override string? Id => HardwareId.HasValue ? "nuki.bridge." + HardwareId : null;

        public override string Title => "Nuki Bridge (" + (HardwareId?.ToString() ?? "?") + ")";

        public string IconName => "fab fa-hubspot";

        // State

        [State]
        public long? HardwareId => Information?.Ids?.HardwareId;

        [State]
        public long? ServerId => Information?.Ids?.ServerId;

        [State]
        public bool IsConnected { get; private set; }

        [State]
        public decimal? PowerConsumption => IsConnected ? 1.0m : 0;

        [State]
        public IEnumerable<IThing> Devices { get; private set; } = Array.Empty<IThing>();

        // Configuration

        [Configuration]
        public string? Host { get; set; }

        [Configuration(IsSecret = true)]
        public string? AuthToken { get; set; }

        [Configuration]
        public int RefreshInterval { get; set; } = 30 * 1000;

        protected override TimeSpan PollingInterval => TimeSpan.FromMilliseconds(RefreshInterval);

        protected override TimeSpan FailureInterval => TimeSpan.FromSeconds(5);

        public NukiBridge(IThingManager thingManager, IHttpClientFactory httpClientFactory, ILogger<NukiBridge> logger)
            : base(thingManager, logger)
        {
            _thingManager = thingManager;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    if (Information == null)
                    {
                        var infoResponse = await httpClient.GetAsync($"http://{Host}/info?token=" + AuthToken, cancellationToken);
                        Information = Newtonsoft.Json.JsonConvert.DeserializeObject<BridgeInformation>(await infoResponse.Content.ReadAsStringAsync(cancellationToken));
                    }

                    var response = await httpClient.GetAsync($"http://{Host}/list?token=" + AuthToken, cancellationToken);
                    var json = await response.Content.ReadAsStringAsync(cancellationToken);
                    var devices = Newtonsoft.Json.JsonConvert.DeserializeObject<NukiDevice[]>(json);

                    Devices = devices
                        .Where(d => d.DeviceType == 0)
                        .CreateOrUpdate(Devices, (a, b) => a.NukiId == b.NukiId, a => new NukiSmartLock(a, this))
                        .ToArray();

                    IsConnected = true;
                    
                    _logger?.LogDebug("Nuki Bridge refreshed.");
                    break;
                }
                catch (Exception exception)
                {
                    IsConnected = false;
                    _logger?.LogError(exception, "Failed to refresh Nuki Bridge.");

                    await Task.Delay(FailureInterval, cancellationToken);
                }
            }
        }
    }
}

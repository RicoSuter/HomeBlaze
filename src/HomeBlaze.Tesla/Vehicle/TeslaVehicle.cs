using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Tesla.Vehicle.Models;
using HomeBlaze.Things;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TeslaAuth;

namespace Baflux.Tesla
{
    [DisplayName("Tesla Vehicle")]
    public class TeslaVehicle : PollingThing, IConnectedDevice
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _logger;

        public override string? Id => Vin != null ? "tesla.vehicle." + Vin : null;

        public override string Title => "Tesla Vehicle (" + (Vin?.ToString() ?? "?") + ")";

        // State

        public bool IsConnected { get; private set; }

        [State]
        public string? Vin => Data?.Vin;

        [ScanForState]
        public VehicleState? Vehicle => Data?.Vehicle_state;

        [ScanForState]
        public DriveState? Drive => Data?.Drive_state;

        [ScanForState]
        public ClimateState? Climate => Data?.Climate_state;

        [ScanForState]
        public ChargeState? Charge => Data?.Charge_state;

        // Configuration

        [Configuration(IsSecret = true)]
        public string? AccessToken { get; set; }

        [Configuration(IsSecret = true)]
        public string? RefreshToken { get; set; }

        [Configuration(IsSecret = true)]
        public DateTimeOffset TokenExpirationDate { get; set; } = DateTimeOffset.MinValue;

        [Configuration]
        public string? VehicleId { get; set; }

        [Configuration]
        public int RefreshInterval { get; set; } = 30 * 1000;

        public TeslaVehicleData? Data { get; private set; }

        protected override TimeSpan PollingInterval => TimeSpan.FromMilliseconds(RefreshInterval);

        protected override TimeSpan FailureInterval => TimeSpan.FromSeconds(5);

        public TeslaVehicle(IThingManager thingManager, IHttpClientFactory httpClientFactory, ILogger<TeslaVehicle> logger) 
            : base(thingManager, logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using var httpClient = await CreateHttpClientAsync(cancellationToken);
                   
                    var versionResponse = await httpClient.GetAsync(
                        $"https://owner-api.teslamotors.com/api/1/vehicles/" + VehicleId + "/vehicle_data", cancellationToken);

                    var json = await versionResponse.Content.ReadAsStringAsync(cancellationToken);
                    if (!json.Contains("vehicle unavailable"))
                    {
                        var data = JsonSerializer.Deserialize<TeslaVehicleDataResponse>(json);
                        Data = data?.Response;

                        IsConnected = Data?.State == "online";
                    }
                    else
                    {
                        if (Vin == null)
                        {
                            await WakeUpAsync(cancellationToken);
                        }

                        IsConnected = false;
                    }

                    _logger.LogDebug("Tesla Vehicle refreshed.");
                    break;
                }
                catch (Exception exception)
                {
                    IsConnected = false;
                    _logger.LogError(exception, "Failed to refresh Tesla Vehicle.");

                    await Task.Delay(FailureInterval, cancellationToken);
                }
            }
        }

        [Operation]
        public async Task WakeUpAsync(CancellationToken cancellationToken)
        {
            using var httpClient = await CreateHttpClientAsync(cancellationToken);
            await httpClient.PostAsync($"https://owner-api.teslamotors.com/api/1/vehicles/" + VehicleId + "/wake_up", null, cancellationToken);
        }

        private async Task<HttpClient> CreateHttpClientAsync(CancellationToken cancellationToken)
        {
            if (RefreshToken != null && TokenExpirationDate - DateTimeOffset.Now < TimeSpan.FromHours(1))
            {
                var auth = new TeslaAuthHelper("Baflux");
                var tokens = await auth.RefreshTokenAsync(RefreshToken, cancellationToken);

                AccessToken = tokens.AccessToken;
                RefreshToken = tokens.RefreshToken;
                TokenExpirationDate = tokens.CreatedAt + tokens.ExpiresIn;
            }

            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            return httpClient;
        }
    }
}
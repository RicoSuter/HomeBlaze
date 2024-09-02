using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Security;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using HomeBlaze.Tesla.Vehicle.Models;
using Microsoft.Extensions.Logging;
using Namotion.Devices.Abstractions.Utilities;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Tesla
{
    [DisplayName("Tesla Vehicle")]
    public class TeslaVehicle : PollingThing, ILastUpdatedProvider,
        IConnectedThing, IAuthenticatedThing, IIconProvider
    {
        private static JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
        };

        private readonly IHttpClientFactory _httpClientFactory;

        public override string Title => "Tesla Vehicle (" + (Vin?.ToString() ?? "?") + ")";

        public bool IsConnected { get; private set; }

        public bool IsAuthenticated => TokenExpirationDate >= DateTimeOffset.Now;

        [State]
        public string? Vin => Response?.Data?.Vin;

        [State]
        public VehicleState? Vehicle => Response?.Data?.VehicleState;

        [State]
        public DriveState? Drive => Response?.Data?.DriveState;

        [State]
        public ClimateState? Climate => Response?.Data?.ClimateState;

        [State]
        public ChargeState? Charge => Response?.Data?.ChargeState;

        [State(Unit = StateUnit.WattHour, IsEstimated = true)]
        public long? EstimatedBatteryCapacity
        {
            get
            {
                var capacity = Charge?.ChargeEnergyAdded > 5m ?
                    (long?)(
                        (Charge?.IdealBatteryRange / Charge?.UsableBatteryLevel * 100) / // ideal battery range at 100%
                        Charge?.ChargeMilesAddedIdeal *
                        Charge?.ChargeEnergyAdded * 1000 // added Watts
                    ) : null;

                return capacity > 10000 ? capacity : null;
            }
        }

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
        public int RefreshInterval { get; set; } = 60 * 1000;

        public TeslaVehicleDataResponse? Response { get; private set; }

        protected override TimeSpan PollingInterval => TimeSpan.FromMilliseconds(RefreshInterval);

        protected override TimeSpan FailureInterval => TimeSpan.FromSeconds(60);

        public DateTimeOffset? LastUpdated { get; set; }

        public string IconName => "fa-solid fa-car";

        public TeslaVehicle(IThingManager thingManager, IHttpClientFactory httpClientFactory, ILogger<TeslaVehicle> logger)
            : base(logger)
        {
            _httpClientFactory = httpClientFactory;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            try
            {
                using var httpClient = await CreateHttpClientAsync(cancellationToken);

                // https://www.teslaapi.io/vehicles/state-and-settings
                var versionResponse = await httpClient.GetAsync(
                    $"https://owner-api.teslamotors.com/api/1/vehicles/" + VehicleId + "/vehicle_data", cancellationToken);

                if (versionResponse.StatusCode != HttpStatusCode.RequestTimeout)
                {
                    versionResponse.EnsureSuccessStatusCode();
                }

                var json = await versionResponse.Content.ReadAsStringAsync(cancellationToken);
                if (json.Contains("vehicle unavailable") == false)
                {
                    Response = JsonUtilities.PopulateOrDeserialize(Response, json, _serializerOptions);

                    if (Response?.Data is not null)
                    {
                        LastUpdated = DateTimeOffset.Now;
                        IsConnected = Response?.Data?.State == "online";
                    }
                    else
                    {
                        IsConnected = false;
                    }
                }
                else
                {
                    IsConnected = false;
                }
            }
            catch
            {
                IsConnected = false;
                TokenExpirationDate = DateTimeOffset.MinValue;
                throw;
            }
        }

        [Operation]
        public async Task WakeUpAsync(CancellationToken cancellationToken)
        {
            using var httpClient = await CreateHttpClientAsync(cancellationToken);

            var response = await httpClient.PostAsync($"https://owner-api.teslamotors.com/api/1/vehicles/" + VehicleId +
                "/wake_up", null, cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        [Operation]
        public async Task SetChargeLimitAsync(
            [OperationParameter(Unit = StateUnit.Percent)] decimal percent,
            CancellationToken cancellationToken)
        {
            using var httpClient = await CreateHttpClientAsync(cancellationToken);

            var response = await httpClient.PostAsync($"https://owner-api.teslamotors.com/api/1/vehicles/" + VehicleId +
                "/command/set_charge_limit",
                JsonContent.Create(new { percent = (int)(percent * 100m) }),
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        [Operation]
        public async Task SetChargeSpeedAsync(decimal chargingAmpere, CancellationToken cancellationToken)
        {
            // see https://github.com/timdorr/tesla-api/blob/master/lib/tesla_api/vehicle.rb#L108

            using var httpClient = await CreateHttpClientAsync(cancellationToken);

            var response = await httpClient.PostAsync($"https://owner-api.teslamotors.com/api/1/vehicles/" + VehicleId +
            "/command/set_charging_amps",
                JsonContent.Create(new { charging_amps = chargingAmpere }),
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        [Operation]
        public async Task SetScheduledChargingAsync(TimeSpan time, CancellationToken cancellationToken)
        {
            using var httpClient = await CreateHttpClientAsync(cancellationToken);

            var response = await httpClient.PostAsync($"https://owner-api.teslamotors.com/api/1/vehicles/" + VehicleId +
                "/command/set_scheduled_charging",
                JsonContent.Create(new { enable = true, time = (int)time.TotalMinutes }),
                cancellationToken);

            // https://github.com/timdorr/tesla-api/blob/master/docs/vehicle/commands/charging.md

            response.EnsureSuccessStatusCode();
        }

        [Operation]
        public async Task OpenChargePortDoorAsync(CancellationToken cancellationToken)
        {
            using var httpClient = await CreateHttpClientAsync(cancellationToken);

            var response = await httpClient.PostAsync($"https://owner-api.teslamotors.com/api/1/vehicles/" + VehicleId +
                "/command/charge_port_door_open",
                JsonContent.Create(new { }),
                cancellationToken);

            var result = response.Content.ReadAsStringAsync(cancellationToken);

            // POST /api/1/vehicles/{id}/command/charge_port_door_open
            // https://github.com/timdorr/tesla-api/blob/master/docs/vehicle/commands/charging.md

            response.EnsureSuccessStatusCode();
        }

        private async Task<HttpClient> CreateHttpClientAsync(CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var tokens = await httpClient.AuthorizeAsync(
                RefreshToken ?? throw new InvalidOperationException("RefreshToken is null."),
                AccessToken ?? throw new InvalidOperationException("AccessToken is null"),
                TokenExpirationDate,
                cancellationToken);

            if (tokens != null)
            {
                AccessToken = tokens.AccessToken;
                RefreshToken = tokens.RefreshToken;
                TokenExpirationDate = tokens.CreatedAt + tokens.ExpiresIn;
            }

            return httpClient;
        }
    }
}
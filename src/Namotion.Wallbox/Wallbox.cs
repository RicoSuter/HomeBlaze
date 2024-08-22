using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Services.Abstractions;

using Namotion.Wallbox.Responses.GetChargerStatus;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Networking;

namespace Namotion.Wallbox
{
    public class Wallbox : PollingThing, 
        IVehicleCharger, 
        IConnectedThing,
        IPowerConsumptionSensor, 
        IIconProvider
    {
        private WallboxClient? _wallboxClient;
        private readonly IHttpClientFactory _httpClientFactory;

        public override string? Title => "Wallbox: " + (Status?.Name ?? SerialNumber);

        string IIconProvider.IconName => "fas fa-plug";

        string IIconProvider.IconColor => 
            IsConnected == false ? "Error" : 
            IsPluggedIn == true ? "Warning" : 
            "Success";

        [Configuration]
        public string SerialNumber { get; set; } = string.Empty;

        [Configuration]
        public string Email { get; set; } = string.Empty;

        [Configuration(IsSecret = true)]
        public string Password { get; set; } = string.Empty;

        [JsonIgnore]
        public bool IsConnected { get; private set; }

        public bool? IsPluggedIn => Status?.Finished == false;

        public bool? IsCharging => Status?.ChargingPower > 1;

        public decimal? PowerConsumption => Status?.ChargingPower * 1000m;

        [State(IsSignal = true)]
        public bool? IsLocked =>
            Status?.ConfigData?.Locked == 1 ? true :
            Status?.ConfigData?.Locked == 0 ? false :
            null;

        [State(Unit = StateUnit.Ampere)]
        public decimal? MaximumChargingCurrent => Status?.ConfigData?.MaximumChargingCurrent;

        [State(Unit = StateUnit.WattHour, IsCumulative = true)]
        public decimal? TotalConsumedEnergy { get; private set; }

        [ScanForState]
        internal GetChargerStatusResponse? Status { get; private set; }

        [Operation]
        public async Task SetMaximumChargingCurrentAsync(int maximumChargingCurrent, CancellationToken cancellationToken)
        {
            if (_wallboxClient is not null &&
                maximumChargingCurrent >= 6 &&
                maximumChargingCurrent <= 32)
            {
                try
                {
                    await _wallboxClient.SetMaximumChargingCurrentAsync(SerialNumber, maximumChargingCurrent, cancellationToken);
                    await PollAsync(cancellationToken);
                }
                finally
                {
                    DetectChanges(this);
                }
            }
        }

        [Operation]
        public async Task LockAsync(CancellationToken cancellationToken)
        {
            if (_wallboxClient is not null)
            {
                try
                {
                    await _wallboxClient.LockAsync(SerialNumber, cancellationToken);
                    await PollAsync(cancellationToken);
                }
                finally
                {
                    DetectChanges(this);
                }
            }
        }

        [Operation]
        public async Task UnlockAsync(CancellationToken cancellationToken)
        {
            if (_wallboxClient is not null)
            {
                try
                {
                    await _wallboxClient.UnlockAsync(SerialNumber, cancellationToken);
                    await PollAsync(cancellationToken);
                }
                finally
                {
                    DetectChanges(this);
                }
            }
        }

        protected override TimeSpan PollingInterval => TimeSpan.FromMinutes(1);

        public Wallbox(IHttpClientFactory httpClientFactory, ILogger<Wallbox> logger)
            : base(logger)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void Reset()
        {
            _wallboxClient = null;
        }

        private DateTimeOffset _lastSessionsRetrieval = DateTimeOffset.MinValue;

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            if (_wallboxClient == null)
            {
                _wallboxClient = new WallboxClient(_httpClientFactory, Email, Password);
            }
            try
            {
                Status = await _wallboxClient.GetChargerStatusAsync(SerialNumber, cancellationToken);
                
                if (DateTimeOffset.UtcNow > _lastSessionsRetrieval.AddMinutes(30) &&
                    Status is not null &&
                    Status.ConfigData?.GroupId is not null)
                {
                    // TODO: We should cache the previous sum and retrieval date and only retrieve new sessions

                    var sessions = await _wallboxClient.GetChargerChargingSessionsAsync(
                        Status.ConfigData.GroupId,
                        Status.ConfigData.ChargerId, 
                        DateTimeOffset.MinValue, 
                        DateTimeOffset.Now, cancellationToken: cancellationToken);

                    _lastSessionsRetrieval = DateTimeOffset.UtcNow;
                   
                    TotalConsumedEnergy = 
                        sessions.Sum(s => s.Attributes.Energy) + 
                        Status.AddedEnergy * 1000;
                }

                IsConnected = true;
            }
            catch
            {
                IsConnected = false;
                throw;
            }
        }
    }
}

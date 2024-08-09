using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System;

using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Services.Abstractions;

using Namotion.Wallbox.Responses.GetChargerStatus;

namespace Namotion.Wallbox
{
    public class Wallbox : PollingThing, IVehicleCharger, IPowerConsumptionSensor
    {
        private WallboxClient? _wallboxClient;
        private readonly IHttpClientFactory _httpClientFactory;

        public override string? Title => "Wallbox: " + (Status?.Name ?? SerialNumber);

        [Configuration]
        public string SerialNumber { get; set; } = string.Empty;

        [Configuration]
        public string Email { get; set; } = string.Empty;

        [Configuration(IsSecret = true)]
        public string Password { get; set; } = string.Empty;

        protected override TimeSpan PollingInterval => TimeSpan.FromMinutes(1);

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

        [ScanForState]
        internal GetChargerStatusResponse? Status { get; private set; }

        [Operation]
        public async Task SetMaximumChargingCurrentAsync(int maximumChargingCurrent, CancellationToken cancellationToken)
        {
            if (_wallboxClient is not null &&
                maximumChargingCurrent >= 6 &&
                maximumChargingCurrent <= 32)
            {
                await _wallboxClient.SetMaximumChargingCurrentAsync(SerialNumber, maximumChargingCurrent, cancellationToken);
                await PollAsync(cancellationToken);
                ThingManager.DetectChanges(this);
            }
        }

        [Operation]
        public async Task LockAsync(CancellationToken cancellationToken)
        {
            if (_wallboxClient is not null)
            {
                await _wallboxClient.LockAsync(SerialNumber, cancellationToken);
                await PollAsync(cancellationToken);
                ThingManager.DetectChanges(this);
            }
        }

        [Operation]
        public async Task UnlockAsync(CancellationToken cancellationToken)
        {
            if (_wallboxClient is not null)
            {
                await _wallboxClient.UnlockAsync(SerialNumber, cancellationToken);
                await PollAsync(cancellationToken);
                ThingManager.DetectChanges(this);
            }
        }

        public Wallbox(IThingManager thingManager, IHttpClientFactory httpClientFactory, ILogger<Wallbox> logger)
            : base(thingManager, logger)
        {
            _httpClientFactory = httpClientFactory;
        }

        public void Reset()
        {
            _wallboxClient = null;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            if (_wallboxClient == null)
            {
                _wallboxClient = new WallboxClient(_httpClientFactory, Email, Password);
            }

            Status = await _wallboxClient.GetChargerStatusAsync(SerialNumber, cancellationToken);
        }
    }
}

using System.Net.Http;
using System.Threading.Tasks;
using Namotion.Wallbox.Responses;
using HomeBlaze.Services.Abstractions;
using System.Threading;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Logging;
using HomeBlaze.Abstractions.Attributes;
using System;
using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Sensors;
using System.Linq;

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

        [ScanForState]
        internal ChargerStatusResponse? Status { get; private set; }

        public decimal? PowerConsumption => Status?.ChargingPower * 1000m;

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

            Status = await _wallboxClient.GetChargerStatusAsync(SerialNumber);
        }
    }
}

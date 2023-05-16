using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.OpenWeatherMap
{
    [DisplayName("OpenWeatherMap Temperature")]
    [ThingSetup(typeof(OpenWeatherMapTemperatureSetup), CanEdit = true)]
    public class OpenWeatherMapTemperature : PollingThing, IIconProvider, ILastUpdatedProvider,
        ITemperatureSensor, IVirtualThing
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public bool IsRefreshing { get; private set; }

        public override string Title => LocationQuery + " (OpenWeatherMap)";

        public string IconName => "fas fa-cloud";

        public DateTimeOffset? LastUpdated { get; private set; }

        // Configuration

        [Configuration(IsSecret = true)]
        public string? ApiKey { get; set; }

        [Configuration("location")]
        public string? LocationQuery { get; set; }

        [Configuration]
        public int RefreshInterval { get; set; } = 15 * 60 * 1000;

        // State

        [State]
        public decimal? Temperature { get; private set; }

        [State]
        public GeoCoordinate? Location => Latitude != null && Longitude != null ? new()
        {
            Longitude = Longitude.Value,
            Latitude = Latitude.Value
        } : null;

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        protected override TimeSpan PollingInterval => TimeSpan.FromMilliseconds(RefreshInterval);

        protected override TimeSpan FailureInterval => TimeSpan.FromSeconds(30);

        public OpenWeatherMapTemperature(
            IThingManager thingManager,
            IHttpClientFactory httpClientFactory,
            ILogger<OpenWeatherMapTemperature> logger)
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
                    if (!string.IsNullOrEmpty(LocationQuery))
                    {
                        using var httpClient = _httpClientFactory.CreateClient();

                        var response = await httpClient.GetAsync(
                            "https://api.openweathermap.org/data/2.5/weather?q=" +
                            Uri.EscapeDataString(LocationQuery) + "&appid=" + ApiKey);

                        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                        var document = await JsonDocument.ParseAsync(stream, default, cancellationToken);

                        // See https://openweathermap.org/current#current_JSON

                        Temperature = document.RootElement
                            .GetProperty("main")
                            .GetProperty("temp")
                            .GetDecimal() - 273.15m;

                        Latitude = document.RootElement
                           .GetProperty("coord")
                           .GetProperty("lat")
                           .GetDouble();

                        Longitude = document.RootElement
                           .GetProperty("coord")
                           .GetProperty("lon")
                           .GetDouble();

                        LastUpdated = DateTimeOffset.Now;
                        ThingManager.DetectChanges(this);
                    }
                }
                finally
                {
                    IsRefreshing = false;
                }
            }
        }
    }
}

using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.ThanksMister
{
    [DisplayName("ThanksMister WallPanel")]
    [Description("WallPanel is an Android application for Web Based Dashboards and Home Automation Platforms.")]
    [ThingSetup(typeof(ThanksMisterWallPanelSetup), CanEdit = true)]
    public class ThanksMisterWallPanel : PollingThing, IIconProvider, 
        IConnectedThing
    {
        // Camera stream: http://192.168.0.87:2971/camera/stream

        private bool _isInitialized = false;
        private IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ThanksMisterWallPanel> _logger;

        public override string? Id => "thanksmister.wallpanel." + InternalId;

        public override string Title => "ThanksMister WallPanel (" + Host + ")";

        public string IconName => "fas fa-tablet-alt";

        // State

        [State]
        public bool IsConnected { get; private set; }

        [State]
        public bool IsScreenOn { get; private set; }

        [State]
        public decimal? Brightness { get; private set; }

        // Configuration

        [Configuration("id")]
        public string? InternalId { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public string? Host { get; set; }

        [Configuration]
        public int Port { get; set; } = 2971;

        [Configuration]
        public bool RelaunchOnStartup { get; set; } = true;

        protected override TimeSpan PollingInterval => TimeSpan.FromSeconds(10);

        protected override TimeSpan FailureInterval => TimeSpan.FromSeconds(60);

        public ThanksMisterWallPanel(IThingManager thingManager, IHttpClientFactory httpClientFactory, ILogger<ThanksMisterWallPanel> logger) 
            : base(thingManager, logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            // See https://github.com/thanksmister/wallpanel-android/wiki/MQTT-and-HTTP-Commands
            try
            {
                if (!string.IsNullOrEmpty(Host))
                {
                    using var httpClient = _httpClientFactory.CreateClient();

                    var url = $"http://{Host}:{Port}/api/state";
                    var response = await httpClient.GetAsync(url, cancellationToken);
                    var text = await response.Content.ReadAsStringAsync(cancellationToken);
                    var json = JsonDocument.Parse(text);

                    IsConnected = true;
                    Brightness = json.RootElement.GetProperty("brightness").GetDecimal() / 255m;
                    IsScreenOn = json.RootElement.GetProperty("screenOn").GetBoolean();

                    await TryInitializeAsync(cancellationToken);
                }
                else
                {
                    IsConnected = false;
                }
            }
            catch (Exception e)
            {
                _logger?.LogInformation(e, "Failed to connect to WallPanel.");
                IsConnected = false;
            }
        }

        private async Task TryInitializeAsync(CancellationToken cancellationToken)
        {
            if (!_isInitialized)
            {
                if (RelaunchOnStartup)
                {
                    try
                    {
                        await WakeUpScreenAsync(cancellationToken);
                        await Task.Delay(2000, cancellationToken);
                        await RelaunchAsync(cancellationToken);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "Could not reload wall panel.");
                    }
                }

                _isInitialized = true;
            }
        }

        [Operation]
        public async Task SpeakAsync(string text, CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var url = $"http://{Host}:{Port}/api/command";
            var json = @"{ ""speak"": " + JsonSerializer.Serialize(text) + @" }";

            await httpClient.PostAsync(url, new StringContent(json), cancellationToken);
        }

        [Operation]
        public async Task PlayAudioAsync(string audioUrl, CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var url = $"http://{Host}:{Port}/api/command";
            var json = @"{ ""audio"": " + JsonSerializer.Serialize(audioUrl) + @" }";
          
            await httpClient.PostAsync(url, new StringContent(json), cancellationToken);
        }

        [Operation]
        public async Task ReloadAsync(CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var url = $"http://{Host}:{Port}/api/command";
            var json = @"{ ""reload"": true }";

            await httpClient.PostAsync(url, new StringContent(json), cancellationToken);
        }

        [Operation]
        public async Task ChangeAudioVolumeAsync(decimal volume, CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var url = $"http://{Host}:{Port}/api/command";
            var json = @"{ ""volume"": " + JsonSerializer.Serialize((int)(volume * 100m)) + @" }";

            await httpClient.PostAsync(url, new StringContent(json), cancellationToken);
        }

        [Operation]
        public async Task ChangeBrightnessAsync(decimal brightness, CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var url = $"http://{Host}:{Port}/api/command";
            var json = @"{ ""brightness"": " + JsonSerializer.Serialize((int)(brightness * 255m)) + @" }";

            await httpClient.PostAsync(url, new StringContent(json), cancellationToken);
        }

        [Operation]
        public async Task RelaunchAsync(CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var url = $"http://{Host}:{Port}/api/command";
            var json = @"{ ""relaunch"": true }";

            await httpClient.PostAsync(url, new StringContent(json), cancellationToken);
        }

        [Operation]
        public async Task WakeUpScreenAsync(CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var url = $"http://{Host}:{Port}/api/command";
            var json = @"{ ""wake"": true }";

            await httpClient.PostAsync(url, new StringContent(json), cancellationToken);
        }
    }
}

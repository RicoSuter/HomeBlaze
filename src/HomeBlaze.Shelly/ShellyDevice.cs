using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using HomeBlaze.Shelly.Model;
using Microsoft.Extensions.Logging;
using MudBlazor;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Shelly
{
    [DisplayName("Shelly Device")]
    [ThingSetup(typeof(ShellyDeviceSetup), CanEdit = true)]
    public class ShellyDevice : PollingThing,
        ILastUpdatedProvider, IIconProvider, IConnectedThing, INetworkAdapter
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ShellyDevice> _logger;

        public override string Title => $"Shelly: {Information?.Name}";

        public DateTimeOffset? LastUpdated { get; private set; }

        public string IconName => "fas fa-box";

        public Color IconColor => IsConnected ? Color.Default : Color.Error;

        [Configuration]
        public string? IpAddress { get; set; }

        [Configuration]
        public int RefreshInterval { get; set; } = 15 * 1000;

        [ScanForState]
        internal ShellyInformation? Information { get; private set; }

        [State]
        public bool IsConnected { get; private set; }

        [State]
        public ShellyCover? Cover { get; private set; }

        protected override TimeSpan PollingInterval => TimeSpan.FromMilliseconds(RefreshInterval);

        public ShellyDevice(
            IThingManager thingManager,
            IHttpClientFactory httpClientFactory,
            ILogger<ShellyDevice> logger)
            : base(thingManager, logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();

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

        internal void Reset()
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

                ThingManager.DetectChanges(this);
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
                Cover = PopulateOrDeserialize(Cover, json);
            }

            LastUpdated = DateTimeOffset.Now;
            IsConnected = true;
        }

        private void Populate<T>(T root, string json)
           where T : class, new()
        {
            JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
                TypeInfoResolver = new PopulateTypeInfoResolver<T>(root)
            });
        }

        private T? PopulateOrDeserialize<T>(T? root, string json)
            where T : class, new()
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
                TypeInfoResolver = new PopulateTypeInfoResolver<T>(root ?? new T())
            });
        }

        private class PopulateTypeInfoResolver<T> : DefaultJsonTypeInfoResolver
            where T : class
        {
            private readonly T _root;

            public PopulateTypeInfoResolver(T root)
            {
                _root = root;
            }

            public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
            {
                var typeInfo = base.GetTypeInfo(type, options);
                if (type == typeof(T))
                {
                    typeInfo.CreateObject = () => _root;
                }
                return typeInfo;
            }
        }
    }
}

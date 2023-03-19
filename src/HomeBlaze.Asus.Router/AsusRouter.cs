using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Asus.Router;
using HomeBlaze.Services.Abstractions;
using HomeBlaze.Services.Extensions;
using Microsoft.Extensions.Logging;
using PixelByProxy.Asus.Router.Configuration;
using PixelByProxy.Asus.Router.Models;
using PixelByProxy.Asus.Router.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.AsusRouter
{
    [DisplayName("ASUS Router")]
    [ThingSetup(typeof(AsusRouterSetup), CanEdit = true)]
    public class AsusRouter : PollingThing,
        ILastUpdatedProvider, IIconProvider,
        IConnectedThing
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public override string? Id => Settings?.LabelMac != null ? "asus.router." + Settings?.LabelMac : null;

        public override string? Title =>
            !string.IsNullOrEmpty(Settings?.LabelMac) ?
            $"ASUS Router ({Settings?.LabelMac})" :
            null;

        public DateTimeOffset? LastUpdated { get; private set; }

        internal AsusService? Service { get; private set; }

        internal RouterSettings? Settings { get; private set; }

        internal Traffic? Traffic { get; private set; }

        public string IconName => "fab fa-hubspot";

        [Configuration]
        public string? Host { get; set; }

        [Configuration]
        public string? Username { get; set; }

        [Configuration(IsSecret = true)]
        public string? Password { get; set; }

        [Configuration]
        public int RefreshInterval { get; set; } = 15 * 1000;

        [State]
        public bool IsConnected { get; private set; }

        [State]
        public string? IpAddress => Settings?.LanIp;

        [State(Unit = StateUnit.Kilobyte)]
        public long? Sent => Traffic?.Sent;

        [State(Unit = StateUnit.Kilobyte)]
        public long? Received => Traffic?.Received;

        [State]
        public AsusRouterClient[] Clients { get; private set; } = Array.Empty<AsusRouterClient>();

        protected override TimeSpan PollingInterval => TimeSpan.FromMilliseconds(RefreshInterval);

        public AsusRouter(
            IThingManager thingManager,
            IHttpClientFactory httpClientFactory,
            ILogger<AsusRouter> logger)
            : base(thingManager, logger)
        {
            _httpClientFactory = httpClientFactory;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (Host != null)
                {
                    if (Service == null)
                    {
                        var config = new AsusConfiguration
                        {
                            HostName = Host,
                            UserName = Username,
                            Password = Password
                        };

                        Service = new AsusService(config, _httpClientFactory.CreateClient());
                        Settings = await Service.GetRouterSettingsAsync(cancellationToken);
                    }

                    Traffic = await Service.GetTrafficAsync(cancellationToken);

                    var clients = await Service.GetClientsAsync(cancellationToken);

                    Clients = clients
                       .CreateOrUpdate(Clients, (a, b) => a.Mac == b.Client.Mac, (a, b) => a.Update(b), a => new AsusRouterClient(a))
                       .ToArray();

                    LastUpdated = DateTimeOffset.Now;
                    IsConnected = true;
                }
            }
            catch
            {
                Service = null;
                IsConnected = false;
                throw;
            }
        }
    }
}

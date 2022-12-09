using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace HomeBlaze.PushOver
{
    [Category("Service")]
    [DisplayName("PushOver (Push Notifications)")]
    [ThingSetup(typeof(PushOverServiceSetup), CanEdit = true)]
    public class PushOverService : IThing, IIconProvider
    {
        internal IHttpClientFactory HttpClientFactory { get; }

        public string IconName => "fab fa-hubspot";

        [Configuration(IsIdentifier = true)]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public string? Title { get; set; } = "PushOver Service";

        [Configuration(IsSecret = true)]
        public string? Token { get; set; }

        [Configuration, State]
        public IList<PushOverTarget> Targets { get; set; } = new ObservableCollection<PushOverTarget>();

        public PushOverService(IHttpClientFactory httpClientFactory)
        {
            HttpClientFactory = httpClientFactory;
        }

        [Operation]
        public async Task SendNotificationAsync(string message)
        {
            foreach (var target in Targets)
            {
                await target.SendNotificationAsync(message);
            }
        }
    }
}

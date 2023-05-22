using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.PushOver
{
    public class PushOverTarget : IThing, IIconProvider, INotificationPublisher
    {
        public string IconName => "fas fa-user";

        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public string? Title { get; set; }

        [Configuration(IsSecret = true)]
        public string? Token { get; set; }

        [ParentThing]
        public PushOverService? Service { get; private set; }

        [Operation]
        public async Task SendNotificationAsync(string message, CancellationToken cancellationToken = default)
        {
            if (Service != null)
            {
                using var httpClient = Service.HttpClientFactory.CreateClient();

                var json = JsonSerializer.Serialize(new
                {
                    token = Service.Token,
                    user = Token,
                    message
                });

                var content = new StringContent(json, new MediaTypeHeaderValue("application/json"));
                var response = await httpClient.PostAsync("https://api.pushover.net/1/messages.json", content, cancellationToken);
                response.EnsureSuccessStatusCode();
            }
        }
    }
}

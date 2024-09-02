using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Gardena
{
    internal class GardenaRestClient
    {
        private readonly string _clientId;
        private readonly string _username;
        private readonly string _password;

        internal string? AccessToken { get; private set; }

        public GardenaRestClient(string clientId, string username, string password)
        {
            _clientId = clientId;
            _username = username;
            _password = password;
        }

        public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            var url = "https://api.authentication.husqvarnagroup.dev/v1/oauth2/token";

            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "client_id", _clientId },
                { "username", _username },
                { "password", _password }
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsync(url, new FormUrlEncodedContent(parameters!), cancellationToken);
                var data = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JObject.Parse(data);
                return result["access_token"]?.Value<string>() 
                    ?? throw new SecurityException($"Authorization failed: \n\n{data}.");
            }
        }

        private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
        {
            if (AccessToken == null)
            {
                AccessToken = await GetAccessTokenAsync(cancellationToken);
            }
        }

        public async Task<Location[]?> GetLocationsAsync(CancellationToken cancellationToken)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", _clientId);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + AccessToken);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization-Provider", "husqvarna");

                    var respone = await client.GetAsync("https://api.smart.gardena.dev/v1/locations", cancellationToken);
                    var data = await respone.Content.ReadAsStringAsync(cancellationToken);
                    var result = JObject.Parse(data);

                    return result["data"]?
                        .OfType<JObject>()
                        .Where(e => e["type"]?.Value<string>() == "LOCATION")
                        .Select(e => new Location
                        {
                            Id = e["id"]?.Value<string>(),
                            Name = e["attributes"]?["name"]?.Value<string>()
                        })
                        .ToArray();
                }
            }
            catch
            {
                AccessToken = null;
                throw;
            }
        }

        public async Task<JObject?> GetLocationAsync(string locationId, CancellationToken cancellationToken)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", _clientId);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + AccessToken);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization-Provider", "husqvarna");

                    var respone = await client.GetAsync("https://api.smart.gardena.dev/v1/locations/" + locationId, cancellationToken);
                    var data = await respone.Content.ReadAsStringAsync();
                    return JObject.Parse(data);
                }
            }
            catch
            {
                AccessToken = null;
                throw;
            }
        }

        public async Task<string?> GetWebSocketAddressAsync(string locationId, CancellationToken cancellationToken)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            try
            {
                // See https://developer.husqvarnagroup.cloud/apis/GARDENA+smart+system+API?tab=readme#sample-websocket-client

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", _clientId);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + AccessToken);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization-Provider", "husqvarna");

                    var respone = await client.PostAsync("https://api.smart.gardena.dev/v1/websocket", new StringContent(@"{
  ""data"": {
    ""id"": """ + Guid.NewGuid().ToString() + @""",
    ""type"": ""WEBSOCKET"",
    ""attributes"": {
      ""locationId"": """ + locationId + @"""
    }
  }
}", Encoding.UTF8, "application/vnd.api+json"), cancellationToken);

                    respone.EnsureSuccessStatusCode();
                    var data = await respone.Content.ReadAsStringAsync(cancellationToken);
                    return JObject.Parse(data)?["data"]?["attributes"]?["url"]?.Value<string>();
                }
            }
            catch
            {
                AccessToken = null;
                throw;
            }
        }

        public async Task SendControlAsync(string serviceId, string controlData, CancellationToken cancellationToken)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", _clientId);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + AccessToken);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization-Provider", "husqvarna");

                    var respone = await client.PutAsync("https://api.smart.gardena.dev/v1/command/" + serviceId,
                        new StringContent(controlData, Encoding.UTF8, "application/vnd.api+json"), cancellationToken);

                    respone.EnsureSuccessStatusCode();
                }
            }
            catch
            {
                AccessToken = null;
                throw;
            }
        }
    }
}

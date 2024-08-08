using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Namotion.Wallbox.Responses;

namespace Namotion.Wallbox
{
    // See https://github.com/tmenguy/wallboxAPIDoc

    public class WallboxClient
    {
        private readonly HttpClient _httpClient;

        private readonly string _baseUrl = "https://api.wall-box.com/";
        private readonly string _email;
        private readonly string _password;

        private string? _wallboxToken;

        private const int ConnectionTimeout = 3000;

        public WallboxClient(IHttpClientFactory httpClientFactory, string email, string password)
        {
            _httpClient = httpClientFactory.CreateClient();
            _email = email;
            _password = password;
        }

        public WallboxClient(HttpClient httpClient, string email, string password)
        {
            _httpClient = httpClient;
            _email = email;
            _password = password;
        }

        private async Task AuthenticateAsync()
        {
            var requestUrl = $"{_baseUrl}auth/token/user";
            var authHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_email}:{_password}"));

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(responseBody);
            _wallboxToken = jsonDocument.RootElement.GetProperty("jwt").GetString();
        }

        public async Task<ChargerStatusResponse?> GetChargerStatusAsync(string chargerId)
        {
            await AuthenticateAsync();

            var requestUrl = $"{_baseUrl}chargers/status/{chargerId}";
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _wallboxToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ChargerStatusResponse>(responseBody);
        }

        public async Task ControlWallboxAsync(string chargerId, string key, object value)
        {
            await AuthenticateAsync();

            var requestUrl = $"{_baseUrl}v2/charger/{chargerId}";
            using var request = new HttpRequestMessage(HttpMethod.Put, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _wallboxToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonSerializer.Serialize(new Dictionary<string, object> { { key, value } }), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task ControlWallboxConfigAsync(string chargerId, string key1, object value1, string? key2 = null, object? value2 = null)
        {
            await AuthenticateAsync();

            var requestUrl = $"{_baseUrl}chargers/config/{chargerId}";
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _wallboxToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var data = new Dictionary<string, object?> { { key1, value1 } };
            if (key2 != null) data.Add(key2, value2);

            request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task PerformRemoteActionAsync(string chargerId, string action, decimal value)
        {
            await AuthenticateAsync();

            var requestUrl = $"{_baseUrl}v3/chargers/{chargerId}/remote-action";
            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _wallboxToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var data = new Dictionary<string, object?> { { action, value } };
            request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task ControlWallboxModesAsync(string chargerId, int percentage, int enabled, int mode)
        {
            await AuthenticateAsync();

            var requestUrl = $"{_baseUrl}v4/chargers/{chargerId}/eco-smart";
            using var request = new HttpRequestMessage(HttpMethod.Put, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _wallboxToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var data = new
            {
                data = new
                {
                    attributes = new
                    {
                        percentage,
                        enabled,
                        mode
                    },
                    type = "eco_smart"
                }
            };
            request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}

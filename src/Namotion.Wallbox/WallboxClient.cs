using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Namotion.Wallbox.Responses;
using System.Linq;
using System.Text.Json.Serialization;

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

        public async Task<string[]> GetGroupUidsAsync()
        {
            await AuthenticateAsync();

            var requestUrl = $"{_baseUrl}v4/space-accesses?limit=999";
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _wallboxToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            var jsonResponse = JsonDocument.Parse(responseBody);
            var groupUids = new List<string>();

            foreach (var item in jsonResponse.RootElement.GetProperty("data").EnumerateArray())
            {
                var groupUid = item.GetProperty("attributes").GetProperty("group_uid").GetString();
                if (groupUid is not null)
                {
                    groupUids.Add(groupUid);
                }
            }

            return groupUids.ToArray();
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

        public async Task<ChargerInformation[]> GetChargersAsync(string groupUuid)
        {
            await AuthenticateAsync();

            var requestUrl = $"{_baseUrl}perseus/organizations/{groupUuid}/chargers?limit=50&offset=0&include=charger_info,charger_config,charger_status&filters=[]";
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _wallboxToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var chargerList = JsonSerializer.Deserialize<WallBoxApiResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return chargerList?.Data?.Select(c => new ChargerInformation
            {
                Id = c.Id,
                Name = c.Attributes?.Name,
                Model = c.Attributes?.Model,
                Status = c.Attributes?.Status ?? 0,
                ConnectionStatus = c.Attributes?.ConnectionStatus,
                LocationName = c.Attributes?.LocationName,
                SerialNumber = c.Attributes?.SerialNumber,
            }).ToArray() ?? Array.Empty<ChargerInformation>();
        }

        public async Task<ChargerInformation[]> GetChargersAsync()
        {
            var groupUids = await GetGroupUidsAsync();
            var chargers = await Task.WhenAll(groupUids.Select(GetChargersAsync));
            return chargers.SelectMany(c => c).ToArray();
        }
    }

    public class WallBoxApiResponse
    {
        [JsonPropertyName("data")]
        public List<ChargerData>? Data { get; set; }
    }

    public class ChargerData
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("attributes")]
        public ChargerAttributes? Attributes { get; set; }
    }

    public class ChargerAttributes
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("status")]
        public int? Status { get; set; }

        [JsonPropertyName("connection_status")]
        public string? ConnectionStatus { get; set; }

        [JsonPropertyName("location_name")]
        public string? LocationName { get; set; }

        [JsonPropertyName("serial_number")]
        public string? SerialNumber { get; set; }
    }

    public class ChargerInformation
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("connection_status")]
        public string? ConnectionStatus { get; set; }

        [JsonPropertyName("location_name")]
        public string? LocationName { get; set; }

        [JsonPropertyName("serial_number")]
        public string? SerialNumber { get; set; }
    }
}

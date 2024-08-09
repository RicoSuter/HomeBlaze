using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

using Namotion.Wallbox.Responses.GetChargers;
using Namotion.Wallbox.Responses.GetChargerStatus;
using System.Threading;

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
        private DateTimeOffset _wallboxTokenExpiration;
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

        public async Task<string[]> GetGroupUidsAsync(CancellationToken cancellationToken)
        {
            return await AuthenticateAsync(async () =>
            {
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
            }, cancellationToken);
        }

        public async Task<ChargerInformation[]> GetChargersAsync(string groupUuid, CancellationToken cancellationToken)
        {
            return await AuthenticateAsync(async () =>
            {
                var requestUrl = $"{_baseUrl}perseus/organizations/{groupUuid}/chargers?limit=50&offset=0&include=charger_info,charger_config,charger_status&filters=[]";
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _wallboxToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var chargerList = JsonSerializer.Deserialize<GetChargersResponse>(responseBody, new JsonSerializerOptions
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
                }).ToArray() ?? [];
            }, cancellationToken);
        }

        public async Task<ChargerInformation[]> GetChargersAsync(CancellationToken cancellationToken)
        {
            var groupUids = await GetGroupUidsAsync(cancellationToken);
            var chargers = await Task.WhenAll(groupUids.Select(g => GetChargersAsync(g, cancellationToken)));
            return chargers.SelectMany(c => c).ToArray();
        }

        public async Task<GetChargerStatusResponse> GetChargerStatusAsync(string chargerId, CancellationToken cancellationToken)
        {
            return await AuthenticateAsync(async () =>
            {
                var requestUrl = $"{_baseUrl}chargers/status/{chargerId}";
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _wallboxToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GetChargerStatusResponse>(responseBody) ?? new GetChargerStatusResponse();
            }, cancellationToken);
        }

        public async Task SetMaximumChargingCurrentAsync(string chargerId, int maximumChargingCurrent, CancellationToken cancellationToken)
        {
            await ControlWallboxAsync(chargerId, "maxChargingCurrent", maximumChargingCurrent, cancellationToken);
        }

        public async Task LockAsync(string chargerId, CancellationToken cancellationToken)
        {
            await ControlWallboxAsync(chargerId, "locked", 1, cancellationToken);
        }

        public async Task UnlockAsync(string chargerId, CancellationToken cancellationToken)
        {
            await ControlWallboxAsync(chargerId, "locked", 0, cancellationToken);
        }

        internal async Task ControlWallboxAsync(string chargerId, string key, object value, CancellationToken cancellationToken)
        {
            await AuthenticateAsync<object>(async () =>
            {
                var requestUrl = $"{_baseUrl}v2/charger/{chargerId}";
                using var request = new HttpRequestMessage(HttpMethod.Put, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _wallboxToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(JsonSerializer.Serialize(new Dictionary<string, object> { { key, value } }), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                return null!;
            }, cancellationToken);
        }

        internal async Task ControlWallboxConfigAsync(string chargerId, string key1, object value1, string? key2 = null, object? value2 = null, CancellationToken cancellationToken = default)
        {
            await AuthenticateAsync<object>(async () =>
            {
                var requestUrl = $"{_baseUrl}chargers/config/{chargerId}";
                using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _wallboxToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var data = new Dictionary<string, object?> { { key1, value1 } };
                if (key2 != null) data.Add(key2, value2);

                request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                return null!;
            }, cancellationToken);
        }

        internal async Task PerformRemoteActionAsync(string chargerId, string action, decimal value, CancellationToken cancellationToken)
        {
            await AuthenticateAsync<object>(async () =>
            {
                var requestUrl = $"{_baseUrl}v3/chargers/{chargerId}/remote-action";
                using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _wallboxToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var data = new Dictionary<string, object?> { { action, value } };
                request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                return null!;
            }, cancellationToken);
        }

        internal async Task ControlWallboxModesAsync(string chargerId, int percentage, int enabled, int mode, CancellationToken cancellationToken)
        {
            await AuthenticateAsync<object>(async () =>
            {
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

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                return null!;
            }, cancellationToken);
        }

        private async Task<T> AuthenticateAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken)
            where T : class
        {
            if (_wallboxToken is null ||
                _wallboxTokenExpiration < DateTimeOffset.UtcNow)
            {
                var requestUrl = $"{_baseUrl}auth/token/user";
                var authHeaderValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_email}:{_password}"));

                using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var jsonDocument = JsonDocument.Parse(responseBody);

                var token = jsonDocument.RootElement.GetProperty("jwt").GetString();
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                _wallboxToken = token;
                _wallboxTokenExpiration = jwtToken.Payload.Expiration.HasValue ?
                    DateTimeOffset.FromUnixTimeSeconds(jwtToken.Payload.Expiration.Value) : DateTimeOffset.MinValue;
            }

            try
            {
                return await action();
            }
            catch
            {
                _wallboxToken = null;
                _wallboxTokenExpiration = DateTimeOffset.MinValue;

                throw;
            }
        }
    }
}

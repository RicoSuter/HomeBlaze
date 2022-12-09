using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using XboxWebApi.Common;
using XboxWebApi.Services;

namespace SmartGlass
{
    public class SmartGlassService : XblService
    {
        private readonly ILogger _logger;

        public SmartGlassService(IXblConfiguration config, ILogger logger)
            : base(config, "https://xccs.xboxlive.com/")
        {
            _logger = logger;
        }

        public async Task LaunchAppAsync(string deviceId, string productId, CancellationToken cancellationToken)
        {
            await SendOneShotCommandAsync(deviceId, "Shell", "ActivateApplicationWithOneStoreProductId", new object[]
            {
                new
                {
                    oneStoreProductId = productId
                }
            }, cancellationToken);
        }

        public async Task SendOneShotCommandAsync(string deviceId, string commandType, string command, object param, CancellationToken cancellationToken)
        {
            var body = new
            {
                destination = "Xbox",
                type = commandType,
                command = command,
                sessionId = Guid.NewGuid().ToString(),
                sourceId = "com.microsoft.smartglass",
                parameters = param ?? new object[] { new object() },
                deviceId = deviceId
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "commands");
            request.Headers.TryAddWithoutValidation("x-xbl-contract-version", "4");
            request.Headers.TryAddWithoutValidation("skillplatform", "RemoteManagement");

            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            using (var response = await HttpClient.SendAsync(request, cancellationToken))
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                // TODO: Handle and improve response
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to send one shot: {Response}.", json);
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public async Task<XboxApp[]> GetInstalledAppsJsonAsync(string deviceId, CancellationToken cancellationToken)
        {
            var body = new
            {
                deviceId
            };

            var result = await GetListAsync<InstalledApps>("installedApps", body, cancellationToken);
            return result.Result ?? Array.Empty<XboxApp>();
        }

        public async Task<XboxConsole[]> GetConsolesAsync(bool includeStorageDevices, CancellationToken cancellationToken)
        {
            var body = new
            {
                queryCurrentDevice = "false",
                includeStorageDevices = includeStorageDevices.ToString().ToLowerInvariant(),
            };

            var result = await GetListAsync<ConsoleList>("devices", body, cancellationToken);
            return result.Result ?? Array.Empty<XboxConsole>();
        }

        public async Task<TResponse> GetListAsync<TResponse>(string listType, object body, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"lists/{listType}");
            request.Headers.TryAddWithoutValidation("x-xbl-contract-version", "4");
            request.Headers.TryAddWithoutValidation("skillplatform", "RemoteManagement");

            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            var response = await HttpClient.SendAsync(request, cancellationToken);
            return await response.Content.ReadAsJsonAsync<TResponse>();
        }
    }

    public partial class XboxApp
    {
        [JsonProperty("oneStoreProductId")]
        public string OneStoreProductId { get; set; }

        [JsonProperty("titleId")]
        public int TitleId { get; set; }

        [JsonProperty("aumid")]
        public string Aumid { get; set; }

        [JsonProperty("lastActiveTime")]
        public object LastActiveTime { get; set; }

        [JsonProperty("isGame")]
        public bool IsGame { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("instanceId")]
        public string InstanceId { get; set; }

        [JsonProperty("storageDeviceId")]
        public string StorageDeviceId { get; set; }

        [JsonProperty("uniqueId")]
        public string UniqueId { get; set; }

        [JsonProperty("legacyProductId")]
        public object LegacyProductId { get; set; }

        [JsonProperty("version")]
        public double Version { get; set; }

        [JsonProperty("sizeInBytes")]
        public double SizeInBytes { get; set; }

        [JsonProperty("installTime")]
        public System.DateTimeOffset InstallTime { get; set; }

        [JsonProperty("updateTime")]
        public System.DateTimeOffset UpdateTime { get; set; }

        [JsonProperty("parentId")]
        public object ParentId { get; set; }
    }

    public partial class InstalledApps
    {
        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("result")]
        public XboxApp[] Result { get; set; }

        [JsonProperty("agentUserId")]
        public object AgentUserId { get; set; }
    }

    public partial class Status
    {
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty("errorMessage")]
        public object ErrorMessage { get; set; }
    }

    public partial class XboxConsole
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("consoleType")]
        public string ConsoleType { get; set; }

        [JsonProperty("powerState")]
        public string PowerState { get; set; }

        [JsonProperty("digitalAssistantRemoteControlEnabled")]
        public bool DigitalAssistantRemoteControlEnabled { get; set; }

        [JsonProperty("remoteManagementEnabled")]
        public bool RemoteManagementEnabled { get; set; }

        [JsonProperty("consoleStreamingEnabled")]
        public bool ConsoleStreamingEnabled { get; set; }
    }

    public partial class ConsoleList
    {
        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("result")]
        public XboxConsole[] Result { get; set; }

        [JsonProperty("agentUserId")]
        public object AgentUserId { get; set; }
    }
}
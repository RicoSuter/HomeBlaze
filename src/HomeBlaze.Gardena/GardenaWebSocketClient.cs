using HomeBlaze.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Gardena
{
    public class GardenaWebSocketClient : ReconnectingWebSocket
    {
        private readonly GardenaLocation _gardenaLocation;
        private readonly ILogger _logger;

        public GardenaWebSocketClient(GardenaLocation gardenaLocation, ILogger logger)
            : base(logger)
        {
            _gardenaLocation = gardenaLocation;
            _logger = logger;
        }

        protected override async Task<string?> GetWebSocketUrlAsync(CancellationToken stoppingToken)
        {
            if (_gardenaLocation.GardenaClient is not null &&
                _gardenaLocation.LocationId is not null)
            {
                return await _gardenaLocation.GardenaClient.GetWebSocketAddressAsync(_gardenaLocation.LocationId, stoppingToken);
            }

            return null;
        }

        protected override Task HandleExceptionAsync(Exception exception, CancellationToken stoppingToken)
        {
            _gardenaLocation.IsAuthenticated = false;
            _gardenaLocation.DetectChanges(_gardenaLocation);
            return Task.CompletedTask;
        }

        protected override Task HandleMessageAsync(string json, CancellationToken stoppingToken)
        {
            try
            {
                var jObj = JObject.Parse(json);

                var gardenaId = jObj["id"]?.Value<string>();
                var type = jObj["type"]?.Value<string>();
                if (!string.IsNullOrEmpty(gardenaId) && !string.IsNullOrEmpty(type))
                {
                    var device = _gardenaLocation.AllDevices.FirstOrDefault(d => d.GardenaId == gardenaId);
                    if (device != null)
                    {
                        if (type == "COMMON")
                        {
                            device.UpdateCommon(jObj);
                        }
                        else
                        {
                            device.Update(jObj);
                        }

                        _gardenaLocation.DetectChanges(device);
                    }
                }

                _gardenaLocation.IsAuthenticated = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse websocket message.");
                _gardenaLocation.IsAuthenticated = false;
            }

            return Task.CompletedTask;
        }
    }
}

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Inputs;
using Microsoft.Extensions.Logging;
using Namotion.Devices.Abstractions.Utilities;
using System;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Namotion.Shelly
{
    public class ShellyWebSocketClient : ReconnectingWebSocket
    {
        private readonly ShellyDevice _device;
        private readonly ILogger _logger;

        public ShellyWebSocketClient(ShellyDevice device, ILogger logger)
            : base(logger)
        {
            _device = device;
            _logger = logger;
        }

        protected override Task<string?> GetWebSocketUrlAsync(CancellationToken stoppingToken)
        {
            return Task.FromResult<string?>($"ws://{_device.IpAddress}/rpc");
        }

        protected async override Task OnConnectedAsync(CancellationToken stoppingToken)
        {
            await SendJsonObjectAsync(
                new { id = 1, src = "Namotion.Shelly:" + _device.Id, method = "Shelly.GetStatus", @params = new { } }, 
                stoppingToken);
        }

        protected override Task HandleMessageAsync(string json, CancellationToken stoppingToken)
        {
            try
            {
                var jsonObject = JsonObject.Parse(json) as JsonObject;
                if (jsonObject?["dst"]?.GetValue<string>() != "Namotion.Shelly:" + _device.Id)
                {
                    return Task.CompletedTask;
                }

                if (jsonObject is not null && 
                    jsonObject["method"] is JsonNode methodProperty &&
                    methodProperty.GetValue<string>() is string method)
                {
                    _logger.LogTrace("WebSocket message received: {Method}", method);
                    switch (method)
                    {
                        case "NotifyStatus":
                            if (jsonObject["params"] is JsonObject paramsObject)
                            {
                                if (paramsObject["em:0"] is JsonObject em0Object)
                                {
                                    _device.EnergyMeter = JsonUtilities.PopulateOrDeserialize(_device.EnergyMeter, em0Object);
                                    _device.EnergyMeter.Update();
                                    _device.DetectChanges(_device);

                                    return Task.CompletedTask;
                                }

                                if (paramsObject["switch:0"] is JsonObject switch0Object)
                                {
                                    _device.Switch0 = JsonUtilities.PopulateOrDeserialize(_device.Switch0, switch0Object);
                                    _device.DetectChanges(_device);

                                    _device.Switch0.IsOnChanged(new SwitchEvent
                                    {
                                        Source = _device,
                                        IsOn = _device.Switch0.IsOn == true
                                    });

                                    return Task.CompletedTask;
                                }

                                if (paramsObject["switch:1"] is JsonObject switch1Object)
                                {
                                    _device.Switch1 = JsonUtilities.PopulateOrDeserialize(_device.Switch1, switch1Object);
                                    _device.DetectChanges(_device);

                                    _device.Switch1.IsOnChanged(new SwitchEvent
                                    {
                                        Source = _device,
                                        IsOn = _device.Switch1.IsOn == true
                                    });

                                    return Task.CompletedTask;
                                }
                            }
                            break;
                    }

                    _logger.LogTrace("Unknown WebSocket message received: {Method}", method);
                }
                else if (jsonObject is not null &&
                    jsonObject["result"] is JsonNode resultProperty)
                {
                    if (resultProperty["em:0"] is JsonObject em0Object)
                    {
                        _device.EnergyMeter = JsonUtilities.PopulateOrDeserialize(_device.EnergyMeter, em0Object);
                        _device.EnergyMeter.Update();
                    }

                    if (resultProperty["switch:0"] is JsonObject switch0Object)
                    {
                        _device.Switch0 = JsonUtilities.PopulateOrDeserialize(_device.Switch0, switch0Object);
                    }

                    if (resultProperty["switch:1"] is JsonObject switch1Object)
                    {
                        _device.Switch1 = JsonUtilities.PopulateOrDeserialize(_device.Switch1, switch1Object);
                    }

                    _device.DetectChanges(_device);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse websocket message.");
            }

            return Task.CompletedTask;
        }

        protected override Task HandleExceptionAsync(Exception exception, CancellationToken stoppingToken)
        {
            _device.IsConnected = false;
            _device.DetectChanges(_device);
            return Task.CompletedTask;
        }
    }
}

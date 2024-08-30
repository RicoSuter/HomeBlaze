using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace HomeBlaze.Abstractions
{
    public abstract class ReconnectingWebSocket : IDisposable
    {
        private readonly ILogger _logger;

        private bool _isRunning = false;

        public ClientWebSocket? WebSocket { get; set; }

        public ReconnectingWebSocket(ILogger logger)
        {
            _logger = logger;
        }

        public void StopWebSocket()
        {
            _isRunning = false;
        }

        protected async Task SendJsonObjectAsync(object jsonObject, CancellationToken stoppingToken)
        {
            if (WebSocket is not null)
            {
                await WebSocket!.SendAsync(
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(jsonObject)),
                    WebSocketMessageType.Binary, true, stoppingToken);
            }
        }

        protected abstract Task<string?> GetWebSocketUrlAsync(CancellationToken stoppingToken);

        protected virtual Task OnConnectedAsync(CancellationToken stoppingToken) { return Task.CompletedTask; }

        protected abstract Task HandleMessageAsync(string json, CancellationToken stoppingToken);

        protected abstract Task HandleExceptionAsync(Exception exception, CancellationToken stoppingToken);

        public void StartWebSocket(CancellationToken stoppingToken)
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Task.Run(async () =>
                {
                    var buffer = System.Net.WebSockets.WebSocket.CreateClientBuffer(100 * 1024, 100 * 1024);
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        if (WebSocket == null)
                        {
                            try
                            {
                                var webSocketAddress = await GetWebSocketUrlAsync(stoppingToken);
                                if (webSocketAddress is not null)
                                {
                                    WebSocket = new ClientWebSocket();
                                    await WebSocket.ConnectAsync(new Uri(webSocketAddress), stoppingToken);
                                    await OnConnectedAsync(stoppingToken);
                                }
                            }
                            catch (Exception exception)
                            {
                                _logger.LogWarning(exception, "Failed to open websocket.");
                                Dispose();
                            }
                        }
                        else
                        {
                            if (WebSocket.State == WebSocketState.Open)
                            {
                                try
                                {
                                    var result = await WebSocket.ReceiveAsync(buffer, stoppingToken);
                                    if (result.Count > 0)
                                    {
                                        var json = Encoding.UTF8.GetString(buffer.Array!, 0, result.Count);
                                        await HandleMessageAsync(json, stoppingToken);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    _logger.LogError(exception, "Failed to receive websocket message.");
                                    await HandleExceptionAsync(exception, stoppingToken);
                                    await Task.Delay(1000);
                                }
                            }
                            else if (WebSocket.State == WebSocketState.Closed)
                            {
                                Dispose();
                            }
                            else
                            {
                                await Task.Delay(1000);
                            }
                        }
                    }
                });
            }
        }

        public void Dispose()
        {
            _isRunning = false;
            WebSocket?.Dispose();
            WebSocket = null;
        }
    }
}

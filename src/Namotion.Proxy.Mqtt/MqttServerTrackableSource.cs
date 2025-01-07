using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Server;
using Namotion.Proxy.Registry;
using Namotion.Proxy.Registry.Abstractions;
using Namotion.Proxy.Sources.Abstractions;

namespace Namotion.Proxy.Mqtt
{
    public class MqttServerTrackableSource<TProxy> : BackgroundService, IProxySource
        where TProxy : IProxy
    {
        private readonly IProxyContext _context;
        private readonly ISourcePathProvider _sourcePathProvider;
        private readonly ILogger _logger;

        private int _numberOfClients = 0;
        private MqttServer? _mqttServer;

        private Action<ProxyPropertyPathReference>? _propertyUpdateAction;
        private ConcurrentDictionary<ProxyPropertyReference, object?> _state = new();

        public int Port { get; set; } = 1883;

        public bool IsListening { get; private set; }

        public int? NumberOfClients => _numberOfClients;

        // TODO: Inject IProxyContext<TProxy> so that multiple contexts are supported.
        public MqttServerTrackableSource(
            IProxyContext context,
            ISourcePathProvider sourcePathProvider,
            ILogger<MqttServerTrackableSource<TProxy>> logger)
        {
            _context = context;
            _sourcePathProvider = sourcePathProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _mqttServer = new MqttFactory()
                .CreateMqttServer(new MqttServerOptions
                {
                    DefaultEndpointOptions =
                    {
                        IsEnabled = true,
                        Port = Port
                    }
                });

            _mqttServer.ClientConnectedAsync += ClientConnectedAsync;
            _mqttServer.ClientDisconnectedAsync += ClientDisconnectedAsync;
            _mqttServer.InterceptingPublishAsync += InterceptingPublishAsync;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _mqttServer.StartAsync();
                    IsListening = true;

                    await Task.Delay(Timeout.Infinite, stoppingToken);
                    await _mqttServer.StopAsync();

                    IsListening = false;
                }
                catch (Exception ex)
                {
                    IsListening = false;

                    if (ex is TaskCanceledException) return;

                    _logger.LogError(ex, "Error in MQTT server.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        public Task<IDisposable?> InitializeAsync(IEnumerable<ProxyPropertyPathReference> properties, Action<ProxyPropertyPathReference> propertyUpdateAction, CancellationToken cancellationToken)
        {
            _propertyUpdateAction = propertyUpdateAction;
            return Task.FromResult<IDisposable?>(null);
        }

        public Task<IEnumerable<ProxyPropertyPathReference>> ReadAsync(IEnumerable<ProxyPropertyPathReference> properties, CancellationToken cancellationToken)
        {
            var propertyPaths = properties
                .Select(p => p.Path)
                .ToList();

            return Task.FromResult<IEnumerable<ProxyPropertyPathReference>>(_state
                .Where(s => propertyPaths.Contains(_sourcePathProvider.TryGetSourcePath(s.Key)!.Replace(".", "/")))
                .Select(s => new ProxyPropertyPathReference(s.Key, null!, s.Value))
                .ToList());
        }

        public async Task WriteAsync(IEnumerable<ProxyPropertyPathReference> propertyChanges, CancellationToken cancellationToken)
        {
            foreach (var property in propertyChanges)
            {
                await _mqttServer!.InjectApplicationMessage(
                    new InjectedMqttApplicationMessage(
                        new MqttApplicationMessage
                        {
                            Topic = string.Join('/', property.Path!.Split('.')),
                            ContentType = "application/json",
                            PayloadSegment = new ArraySegment<byte>(
                               Encoding.UTF8.GetBytes(JsonSerializer.Serialize(property.Value)))
                        }));
            }
        }

        public string? TryGetSourcePath(ProxyPropertyReference property)
        {
            return _sourcePathProvider.TryGetSourcePath(property);
        }

        private Task ClientConnectedAsync(ClientConnectedEventArgs arg)
        {
            _numberOfClients++;

            Task.Run(async () =>
            {
                await Task.Delay(1000);
                foreach (var property in _context
                    .GetHandler<IProxyRegistry>()
                    .GetProperties()
                    .Where(p => p.HasGetter))
                {
                    await PublishPropertyValueAsync(property.GetValue(), property.Property);
                }
            });

            return Task.CompletedTask;
        }

        private async Task PublishPropertyValueAsync(object? value, ProxyPropertyReference property)
        {
            var sourcePath = _sourcePathProvider.TryGetSourcePath(property);
            if (sourcePath != null)
            {
                await _mqttServer!.InjectApplicationMessage(new InjectedMqttApplicationMessage(new MqttApplicationMessage
                {
                    Topic = string.Join('/', sourcePath.Split('.')),
                    ContentType = "application/json",
                    PayloadSegment = new ArraySegment<byte>(
                        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value)))
                }));
            }
        }

        private Task InterceptingPublishAsync(InterceptingPublishEventArgs args)
        {
            try
            {
                var sourcePath = args.ApplicationMessage.Topic.Replace('/', '.');
                var property = _context
                    .GetHandler<IProxyRegistry>()
                    .GetProperties()
                    .SingleOrDefault(p => _sourcePathProvider.TryGetSourcePath(p.Property) == sourcePath);

                if (property != default)
                {
                    var payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);
                    var document = JsonDocument.Parse(payload);
                    var value = document.Deserialize(property.Type);

                    _state[property.Property] = value;
                    _propertyUpdateAction?.Invoke(new ProxyPropertyPathReference(property.Property, sourcePath, value));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize MQTT payload.");
            }

            return Task.CompletedTask;
        }

        private Task ClientDisconnectedAsync(ClientDisconnectedEventArgs arg)
        {
            _numberOfClients--;
            return Task.CompletedTask;
        }
    }
}
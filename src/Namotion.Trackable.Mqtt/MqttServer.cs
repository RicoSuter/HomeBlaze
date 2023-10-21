using Microsoft.Extensions.Hosting;
using MQTTnet.Server;
using MQTTnet;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Namotion.Trackable;
using System.Reactive.Linq;
using Namotion.Trackable.Model;
using Namotion.Trackable.Sourcing;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace HomeBlaze.Mqtt
{
    public class MqttServer<TTrackable> : BackgroundService, ITrackableContextSource
        where TTrackable : class
    {
        private readonly TrackableContext<TTrackable> _trackableContext;
        private readonly ILogger _logger;

        private int _numberOfClients = 0;
        private MqttServer? _mqttServer;

        private Action<string, object?>? _propertyUpdateAction;
        private ConcurrentDictionary<string, object?> _state = new ConcurrentDictionary<string, object?>();

        public int Port { get; set; } = 1883;

        public bool IsListening { get; set; }

        public int? NumberOfClients => _numberOfClients;

        public MqttServer(TrackableContext<TTrackable> trackableContext, ILogger<MqttServer<TTrackable>> logger)
        {
            _trackableContext = trackableContext;
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

                    _logger.LogError(ex, "Error in MQTT server.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task PublishPropertyValueAsync(object? value, TrackedProperty property)
        {
            var sourcePath = property.TryGetSourcePath(_trackableContext);
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

        private Task ClientConnectedAsync(ClientConnectedEventArgs arg)
        {
            _numberOfClients++;

            Task.Run(async () =>
            {
                await Task.Delay(1000);
                foreach (var property in _trackableContext.AllProperties)
                {
                    await PublishPropertyValueAsync(property.GetSourceValue(), property);
                }
            });

            return Task.CompletedTask;
        }

        private Task ClientDisconnectedAsync(ClientDisconnectedEventArgs arg)
        {
            _numberOfClients--;
            return Task.CompletedTask;
        }

        private Task InterceptingPublishAsync(InterceptingPublishEventArgs args)
        {
            try
            {
                var sourcePath = args.ApplicationMessage.Topic.Replace('/', '.');
                var property = _trackableContext
                    .AllProperties
                    .SingleOrDefault(p => p.TryGetSourcePath() == sourcePath);

                if (property != null)
                {
                    var payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);
                    var document = JsonDocument.Parse(payload);
                    var value = document.Deserialize(property.PropertyType);

                    _state[args.ApplicationMessage.Topic] = value;
                    _propertyUpdateAction?.Invoke(sourcePath, value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize MQTT payload.");
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyDictionary<string, object?>> ReadAsync(IEnumerable<string> sourcePaths, CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyDictionary<string, object?>>(_state
                .Where(s => sourcePaths.Contains(s.Key.Replace(".", "/")))
                .ToDictionary(s => s.Key, s => s.Value));
        }

        public Task<IDisposable?> SubscribeAsync(IEnumerable<string> sourcePaths, Action<string, object?> propertyUpdateAction, CancellationToken cancellationToken)
        {
            _propertyUpdateAction = propertyUpdateAction;
            return Task.FromResult<IDisposable?>(null);
        }

        public async Task WriteAsync(IReadOnlyDictionary<string, object?> propertyChanges, CancellationToken cancellationToken)
        {
            foreach ((var sourcePath, var value) in propertyChanges)
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
    }
}
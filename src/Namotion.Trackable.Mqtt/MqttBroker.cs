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

namespace HomeBlaze.Mqtt
{
    public class MqttBroker<TTrackable> : BackgroundService
        where TTrackable : class
    {
        private readonly TrackableContext<TTrackable> _trackableContext;
        private readonly ILogger _logger;

        private int _numberOfClients = 0;
        private MqttServer? _mqttServer;

        public string IconName => "fas fa-envelope";

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title { get; set; } = "MQTT Broker";

        public int Port { get; set; } = 1883;

        public bool IsListening { get; set; }

        public int? NumberOfClients => _numberOfClients;

        public MqttBroker(TrackableContext<TTrackable> trackableContext, ILogger<MqttBroker<TTrackable>> logger)
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

                    await _trackableContext
                        .ForEachAsync(async change => await PropertyChangedAsync(change), stoppingToken);

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

        private async Task PropertyChangedAsync(TrackedPropertyChange change)
        {
            var property = change.Property;
            await PublishPropertyValueAsync(change.Value, property);
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
                    property.SetValueFromSource(document.Deserialize(property.PropertyType));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize MQTT payload.");
            }

            return Task.CompletedTask;
        }
    }
}
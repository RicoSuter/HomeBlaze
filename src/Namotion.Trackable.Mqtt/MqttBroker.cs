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
using System.Text.Json.Serialization;
using System.Text.Json;

namespace HomeBlaze.Mqtt
{
    public class MqttBroker<TTrackable> : BackgroundService
        where TTrackable : class
    {
        private readonly TrackableContext<TTrackable> _trackableContext;
        private readonly ILogger _logger;

        private int _numberOfClients = 0;

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
            var mqttServer = new MqttFactory()
                .CreateMqttServer(new MqttServerOptions
                {
                    DefaultEndpointOptions =
                    {
                        IsEnabled = true,
                        Port = Port
                    }
                });

            mqttServer.ClientConnectedAsync += ClientConnectedAsync;
            mqttServer.ClientDisconnectedAsync += ClientDisconnectedAsync;
            mqttServer.InterceptingPublishAsync += InterceptingPublishAsync;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await mqttServer.StartAsync();
                    IsListening = true;

                    await _trackableContext
                        .ForEachAsync(async change => await PropertyChangedAsync(change, mqttServer), stoppingToken);

                    await Task.Delay(Timeout.Infinite, stoppingToken);
                    await mqttServer.StopAsync();

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

        private async Task PropertyChangedAsync(TrackedPropertyChange change, MqttServer mqttServer)
        {
            var sourcePath = change.Property.TryGetSourcePath();
            if (sourcePath != null)
            {
                await mqttServer.InjectApplicationMessage(new InjectedMqttApplicationMessage(new MqttApplicationMessage
                {
                    Topic = sourcePath,
                    ContentType = "application/json",
                    PayloadSegment = new ArraySegment<byte>(
                        System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(change.Value)))
                }));
            }
        }

        private Task ClientConnectedAsync(ClientConnectedEventArgs arg)
        {
            _numberOfClients++;
            return Task.CompletedTask;
        }

        private Task ClientDisconnectedAsync(ClientDisconnectedEventArgs arg)
        {
            _numberOfClients--;
            return Task.CompletedTask;
        }

        private Task InterceptingPublishAsync(InterceptingPublishEventArgs args)
        {


            return Task.CompletedTask;
        }
    }
}
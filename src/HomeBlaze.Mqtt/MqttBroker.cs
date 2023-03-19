using Microsoft.Extensions.Hosting;
using MQTTnet.Server;
using MQTTnet;
using System;
using System.Threading;
using System.Threading.Tasks;
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using System.Collections.Generic;
using System.Linq;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Logging;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Networking;
using System.ComponentModel;

namespace HomeBlaze.Mqtt
{
    [DisplayName("MQTT Broker")]
    [ThingSetup(typeof(MqttBrokerSetup), CanEdit = true)]
    public class MqttBroker : BackgroundService, IIconProvider, IThing, IServerThing
    {
        private readonly IThingManager _thingManager;
        private readonly ILogger<MqttBroker> _logger;
        private int _numberOfClients = 0;

        public string IconName => "fas fa-envelope";

        [Configuration]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public string? Title { get; set; } = "MQTT Broker";

        [Configuration]
        public int Port { get; set; } = 1883;

        [State]
        public List<MqttTopic> Topics { get; } = new List<MqttTopic>();

        public bool IsListening { get; set; }

        public int? NumberOfClients => _numberOfClients;

        public MqttBroker(IThingManager thingManager, ILogger<MqttBroker> logger)
        {
            _thingManager = thingManager;
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

            _thingManager.DetectChanges(this);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await mqttServer.StartAsync();
                  
                    IsListening = true;
                    _thingManager.DetectChanges(this);

                    await Task.Delay(Timeout.Infinite, stoppingToken);
                    await mqttServer.StopAsync();

                    IsListening = false;
                    _thingManager.DetectChanges(this);
                }
                catch (Exception ex)
                {
                    IsListening = false;
                    _thingManager.DetectChanges(this);

                    _logger.LogError(ex, "Error in MQTT server.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
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
            var topics = args.ApplicationMessage.Topic.Split('/');
            if (topics.Any())
            {
                var topic = Topics.SingleOrDefault(t => t.Title == topics.First());
                if (topic == null)
                {
                    topic = new MqttTopic
                    {
                        Id = Id + "/" + topics.First(),
                        Title = topics.First()
                    };

                    Topics.Add(topic);
                    _thingManager.DetectChanges(this);
                }

                topic.Apply(topics.Skip(1), args, _thingManager);
            }

            return Task.CompletedTask;
        }
    }
}
using Microsoft.Extensions.Hosting;
using MQTTnet.Server;
using MQTTnet;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using System.Collections.Generic;
using System.Linq;
using HomeBlaze.Abstractions.Services;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace HomeBlaze.Mqtt
{
    public class MqttTopic : IThing, IStateProvider
    {
        private Dictionary<string, object?> _state = new Dictionary<string, object?>();

        public string? Id { get; set; }

        public string? Title { get; set; }

        [State]
        public List<MqttTopic> Topics { get; } = new List<MqttTopic>();

        public void Apply(IEnumerable<string> topics, InterceptingPublishEventArgs args, IThingManager thingManager)
        {
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
                    thingManager.DetectChanges(this);
                }

                topic.Apply(topics.Skip(1), args, thingManager);
            }
            else
            {
                _state.Clear();

                var payload = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
                try
                {
                    var document = JsonDocument.Parse(payload);
                    if (document.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var property in document.RootElement.EnumerateObject())
                        {
                            if (property.Value.ValueKind == JsonValueKind.String)
                            {
                                _state[property.Name] = property.Value.GetString();
                            } 
                            else if (property.Value.ValueKind == JsonValueKind.Number)
                            {
                                _state[property.Name] = property.Value.GetDecimal();
                            }
                            else if (property.Value.ValueKind == JsonValueKind.True)
                            {
                                _state[property.Name] = true;
                            }
                            else if (property.Value.ValueKind == JsonValueKind.False)
                            {
                                _state[property.Name] = false;
                            }

                            // TODO: Handle arrays and objects?
                        }
                    }
                }
                catch
                {
                    // not JSON
                    _state["Payload"] = payload;
                }

                thingManager.DetectChanges(this);
            }
        }

        public IReadOnlyDictionary<string, object?> GetState()
        {
            return _state;
        }
    }

    [ThingSetup(typeof(MqttBrokerSetup), CanEdit = true)]
    public class MqttBroker : BackgroundService, IThing
    {
        private readonly IThingManager _thingManager;
        private readonly ILogger<MqttBroker> _logger;

        [Configuration]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public string? Title { get; set; } = "MQTT Broker";

        [State]
        public List<MqttTopic> Topics { get; } = new List<MqttTopic>();

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
                    }
                });

            mqttServer.ClientConnectedAsync += ClientConnectedAsync;
            mqttServer.InterceptingPublishAsync += InterceptingPublishAsync;

            _thingManager.DetectChanges(this);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await mqttServer.StartAsync();
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                    await mqttServer.StopAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in MQTT server.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private Task ClientConnectedAsync(ClientConnectedEventArgs arg)
        {
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
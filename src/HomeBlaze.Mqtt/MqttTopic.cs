using MQTTnet.Server;
using System.Text;
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using System.Collections.Generic;
using System.Linq;
using HomeBlaze.Abstractions.Services;
using System.Text.Json;
using HomeBlaze.Abstractions.Presentation;

namespace HomeBlaze.Mqtt
{
    public class MqttTopic : IThing, IIconProvider, IStateProvider
    {
        private Dictionary<string, object?> _state = new Dictionary<string, object?>();

        public required string Id { get; init; }

        public string? Title { get; set; }

        public string IconName => "fas fa-envelope";

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

                var payload = Encoding.UTF8.GetString(args.ApplicationMessage.PayloadSegment);
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
}
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Json;
using Namotion.Devices.Abstractions.Utilities;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeBlaze.Services
{
    public class ThingSerializer : IThingSerializer
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public ThingSerializer(IServiceProvider serviceProvider)
        {
            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() },
                TypeInfoResolver = new ThingTypeInfoResolver(serviceProvider, serializeIdentifier: false),
            };
        }

        public T CloneThing<T>(T thing)
            where T : IThing
        {
            return JsonUtilities.Clone(thing, _serializerOptions);
        }

        public string SerializeThing<T>(T source) where T : IThing
        {
            return JsonSerializer.Serialize(source, _serializerOptions);
        }

        public void PopulateThing<T>(T target, string sourceJson) where T : IThing
        {
            JsonUtilities.Populate(target, sourceJson, _serializerOptions);
        }

        public void PopulateThing<T>(T source, T target) where T : IThing
        {
            var json = JsonSerializer.Serialize(source, _serializerOptions);
            JsonUtilities.Populate(target, json, _serializerOptions);
        }
    }
}
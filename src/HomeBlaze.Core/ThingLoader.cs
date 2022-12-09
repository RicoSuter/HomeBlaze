using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Json;
using Namotion.Storage;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeBlaze
{
    public class ThingRepository : IThingStorage
    {
        private readonly JsonSerializerOptions _thingSerializerOptions;
        private readonly JsonSerializerOptions _thingCloneSerializerOptions;

        private readonly IBlobContainer _blobContainer;
        private readonly IServiceProvider _serviceProvider;

        public ThingRepository(IBlobContainer blobContainer, IServiceProvider serviceProvider)
        {
            _blobContainer = blobContainer;
            _serviceProvider = serviceProvider;

            _thingSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() },
                TypeInfoResolver = new ThingTypeInfoResolver(_serviceProvider, serializeIdentifier: true),
            };

            _thingCloneSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() },
                TypeInfoResolver = new ThingTypeInfoResolver(_serviceProvider, serializeIdentifier: false),
            };
        }

        public async Task<IThing> ReadThingAsync(CancellationToken cancellationToken)
        {
            var json = await _blobContainer.ReadAllTextAsync("Root.json", cancellationToken);

            var rootThing = JsonSerializer.Deserialize<IThing>(json, _thingSerializerOptions);
            if (rootThing == null)
            {
                throw new InvalidOperationException("Could not deserialize root.");
            }

            return rootThing;
        }

        public async Task WriteThingAsync(IThing thing, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(thing, _thingSerializerOptions);
            await _blobContainer.WriteAllTextAsync("Root.json", json, cancellationToken);
        }

        public T CloneThing<T>(T thing)
            where T : IThing
        {
            return JsonUtilities.Clone(thing, _thingCloneSerializerOptions);
        }

        public void PopulateThing<T>(T source, T target) where T : IThing
        {
            var json = JsonSerializer.Serialize(source, _thingCloneSerializerOptions);
            JsonUtilities.PopulateObject(json, target, _thingCloneSerializerOptions);
        }
    }
}
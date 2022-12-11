using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Json;
using Namotion.Storage;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeBlaze.Services
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

        public async Task<IRootThing> ReadRootThingAsync(CancellationToken cancellationToken)
        {
            var json = await ReadRootConfigurationAsync(cancellationToken);

            var rootThing = JsonSerializer.Deserialize<IThing>(json, _thingSerializerOptions);
            if (rootThing is not IRootThing)
            {
                throw new InvalidOperationException("Could not deserialize root.");
            }

            return (IRootThing)rootThing;
        }

        private async Task<string> ReadRootConfigurationAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _blobContainer.ReadAllTextAsync("Root.json", cancellationToken);
            }
            catch (BlobNotFoundException)
            {
                return "{\"discriminator\": \"HomeBlaze.Things.SystemThing\"}";
            }
        }

        public async Task WriteRootThingAsync(IRootThing thing, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize<IThing>(thing, _thingSerializerOptions);
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
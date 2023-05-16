using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Json;
using Namotion.Storage;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HomeBlaze.Services
{
    public class ThingStorage : IThingStorage
    {
        private readonly JsonSerializerOptions _thingSerializerOptions;
        private readonly IBlobContainer _blobContainer;

        public ThingStorage(IBlobContainer blobContainer, IServiceProvider serviceProvider)
        {
            _blobContainer = blobContainer;
            _thingSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() },
                TypeInfoResolver = new ThingTypeInfoResolver(serviceProvider, serializeIdentifier: true),
            };
        }

        public async Task<IGroupThing> ReadRootThingAsync(CancellationToken cancellationToken)
        {
            var json = await ReadRootConfigurationAsync(cancellationToken);

            var rootThing = JsonSerializer.Deserialize<IThing>(json, _thingSerializerOptions);
            if (rootThing is not IGroupThing)
            {
                throw new InvalidOperationException("Could not deserialize root.");
            }

            return (IGroupThing)rootThing;
        }

        private async Task<string> ReadRootConfigurationAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _blobContainer.ReadAllTextAsync("Root.json", cancellationToken);
            }
            catch (BlobNotFoundException)
            {
                // TODO: Should not have dependency on type in HomeBlaze
                return "{\"discriminator\": \"HomeBlaze.Things.SystemThing\"}";
            }
        }

        public async Task WriteRootThingAsync(IGroupThing thing, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize<IThing>(thing, _thingSerializerOptions);
            await _blobContainer.WriteAllTextAsync("Root.json", json, cancellationToken);
        }
    }
}
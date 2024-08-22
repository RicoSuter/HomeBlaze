using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Abstractions.Presentation;

using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeBlaze.Dynamic
{
    [DisplayName("Static Properties")]
    [Description("Add static properties to a thing.")]
    [ThingSetup(typeof(StaticPropertiesThingSetup), CanEdit = true, CanClone = true)]
    public class StaticPropertiesThing : IExtensionThing, IStateProvider, IIconProvider
    {
        private readonly IThingManager _thingManager;

        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title => $"{ExtendedThing?.Title}: Static Properties";

        public string IconName => "fa-solid fa-rectangle-list";

        [Configuration]
        public string? ExtendedThingId { get; set; }

        public IThing? ExtendedThing => _thingManager.TryGetById(ExtendedThingId);

        [Configuration]
        public Dictionary<string, string?> Properties { get; set; } = new Dictionary<string, string?>();

        public StaticPropertiesThing(IThingManager thingManager)
        {
            _thingManager = thingManager;
        }

        public IReadOnlyDictionary<string, object?> GetState()
        {
            return Properties.ToDictionary(x => x.Key, x => (object?)x.Value);
        }
    }
}
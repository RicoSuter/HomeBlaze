using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using System;
using System.ComponentModel;

namespace HomeBlaze.Widgets
{
    [DisplayName("Property")]
    [ThingSetup(typeof(PropertyWidgetSetup))]
    [ThingWidget(typeof(PropertyWidgetComponent))]
    public class PropertyWidget : IThing
    {
        private readonly IThingManager _thingManager;

        [Configuration(IsIdentifier = true)]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public string? Title { get; set; }

        [Configuration]
        public string? ThingId { get; set; }

        public IThing? Thing => _thingManager.TryGetById(ThingId);

        [Configuration]
        public string? PropertyName { get; set; }

        [Configuration]
        public string? Icon { get; set; }

        [Configuration]
        public string? Unit { get; set; }

        [Configuration]
        public decimal Scale { get; set; } = 1.0m;

        [Configuration]
        public decimal Multiplier { get; set; } = 1m;

        [Configuration]
        public int Decimals { get; set; } = 2;

        public PropertyWidget(IThingManager thingManager)
        {
            _thingManager = thingManager;
        }
    }
}
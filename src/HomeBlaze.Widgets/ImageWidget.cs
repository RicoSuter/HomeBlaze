using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using System;
using System.ComponentModel;

namespace HomeBlaze.Widgets
{
    [DisplayName("Image")]
    [ThingWidget(typeof(ImageWidgetComponent))]
    public class ImageWidget : IThing
    {
        private readonly IThingManager _thingManager;

        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public string? Title { get; set; }

        [Configuration]
        public string? ImageUrl { get; set; }

        [Configuration]
        public string? ThingId { get; set; }

        public IThing? Thing => _thingManager.TryGetById(ThingId);

        [Configuration]
        public string? PropertyName { get; set; }

        [Configuration]
        public decimal Width { get; set; } = 200;

        [Configuration]
        public decimal Height { get; set; } = 100;

        public ImageWidget(IThingManager thingManager)
        {
            _thingManager = thingManager;
        }
    }
}
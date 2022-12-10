using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using System;
using System.ComponentModel;

namespace HomeBlaze.Widgets
{
    [DisplayName("Toggle Button")]
    [ThingSetup(typeof(ToggleButtonWidgetSetup))]
    [ThingWidget(typeof(ToggleButtonWidgetComponent))]
    public class ToggleButtonWidget : IThing
    {
        [Configuration(IsIdentifier = true)]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title => Label;

        [Configuration]
        public string? Label { get; set; }

        [Configuration]
        public string? Description { get; set; }

        [Configuration]
        public int VerticalTextPosition { get; set; }

        [Configuration]
        public int Width { get; set; } = 100;

        [Configuration]
        public int Height { get; set; } = 100;

        [Configuration]
        public string? ThingId { get; set; }
    }
}

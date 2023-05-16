using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using System;
using System.ComponentModel;

namespace HomeBlaze.Widgets.Specialized
{
    [DisplayName("Power Consumers Table")]
    [ThingSetup(typeof(PowerConsumersWidgetSetup))]
    [ThingWidget(typeof(PowerConsumersWidgetComponent))]
    public class PowerConsumersWidget : IThing
    {
        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title => "Power Consumers Table Widget";

        [Configuration]
        public int Width { get; set; } = 600;

        [Configuration]
        public int Height { get; set; } = 400;
    }
}

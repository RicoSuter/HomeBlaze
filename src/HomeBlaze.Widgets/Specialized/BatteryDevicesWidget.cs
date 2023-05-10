using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using System;
using System.ComponentModel;

namespace HomeBlaze.Widgets.Specialized
{
    [DisplayName("Battery Devices Table")]
    [ThingSetup(typeof(BatteryDevicesWidgetSetup))]
    [ThingWidget(typeof(BatteryDevicesWidgetComponent))]
    public class BatteryDevicesWidget : IThing
    {
        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title => "Battery Devices Table Widget";

        [Configuration]
        public int Width { get; set; } = 600;

        [Configuration]
        public int Height { get; set; } = 400;
    }
}

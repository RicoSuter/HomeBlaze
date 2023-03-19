using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Components.Editors;
using System;
using System.ComponentModel;

namespace HomeBlaze.Widgets
{
    [DisplayName("Button")]
    [ThingSetup(typeof(ButtonWidgetSetup))]
    [ThingWidget(typeof(ButtonWidgetComponent))]
    public class ButtonWidget : IThing
    {
        [Configuration(IsIdentifier = true)]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title => Label;

        [Configuration]
        public string? Label { get; set; }

        [Configuration]
        public int Width { get; set; } = 100;

        [Configuration]
        public int Height { get; set; } = 100;

        [Configuration]
        public Operation Operation { get; set; } = new Operation();
    }
}

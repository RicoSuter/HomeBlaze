using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Components.Editors;
using System;
using System.ComponentModel;

namespace HomeBlaze.Widgets
{
    [DisplayName("Switch Button")]
    [ThingWidget(typeof(SwitchButtonWidgetComponent))]
    public class SwitchButtonWidget : IThing
    {
        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

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

        [Configuration]
        public bool AllowTurnOn { get; set; } = true;

        [Configuration]
        public bool AllowTurnOff { get; set; } = true;

        [Configuration]
        public string? IsOnExpression { get; set; }

        [Configuration]
        public Operation? TurnOnOperation { get; set; }

        [Configuration]
        public Operation? TurnOffOperation { get; set; }
    }
}

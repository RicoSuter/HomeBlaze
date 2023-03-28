using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions;
using System.ComponentModel;
using System;

namespace HomeBlaze.Widgets
{
    [DisplayName("Aggregation")]
    [ThingSetup(typeof(AggregationWidgetSetup))]
    [ThingWidget(typeof(AggregationWidgetComponent))]
    public class AggregationWidget : IThing
    {
        [Configuration(IsIdentifier = true)]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title => Label;

        [Configuration]
        public string? Label { get; set; } = "Rain";

        [Configuration]
        public string? Icon { get; set; } = "fas fa-cloud-rain";

        [Configuration]
        public string? Unit { get; set; } = "mm/Day";

        [Configuration]
        public decimal Scale { get; set; } = 1.0m;

        [Configuration]
        public decimal ExpectedMaximum { get; set; } = 100m;

        [Configuration]
        public decimal Multiplier { get; set; } = 1m;

        [Configuration]
        public int Days { get; set; } = 7;

        [Configuration]
        public int Decimals { get; set; } = 2;

        [Configuration(IsThingReference = true)]
        public string? ThingId { get; set; }

        [Configuration]
        public string? PropertyName { get; set; }
    }
}
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace HomeBlaze.Widgets
{
    [DisplayName("Icon")]
    [ThingWidget(typeof(IconWidgetComponent), NoPointerEvents = true)]
    public class IconWidget : IThing
    {
        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title => "Icon";

        [Configuration]
        public decimal Scale { get; set; } = 1.0m;

        [Configuration]
        public decimal Size { get; set; } = 1.0m;

        [Configuration]
        public List<IconCondition> Conditions { get; set; } = new List<IconCondition> 
        {
            new IconCondition()
        };

        public class IconCondition
        {
            public string? Icon { get; set; } = "fas fa-question-circle";

            public decimal Opacity { get; set; } = 1.0m;

            public string Color { get; set; } = "#FFFFFF";

            public string? ThingId { get; set; }

            public string? PropertyName { get; set; }

            public string? Value { get; set; }

            public bool IsActive(IThingManager thingManager)
            {
                if (ThingId == null)
                {
                    return true;
                }

                if (PropertyName != null)
                {
                    var propertyState = thingManager.TryGetPropertyState(ThingId, PropertyName, true);
                    return propertyState?.Value?.ToString()?.ToLowerInvariant() == Value?.ToLowerInvariant();
                }

                return false;
            }
        }
    }
}

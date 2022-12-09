using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using System.ComponentModel;

namespace HomeBlaze.Widgets
{
    [DisplayName("Icon")]
    [ThingSetup(typeof(IconWidgetSetup))]
    [ThingWidget(typeof(IconWidgetComponent), NoPointerEvents = true)]
    public class IconWidget : IThing
    {
        [Configuration(IsIdentifier = true)]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title => "Icon";

        [Configuration]
        public decimal Scale { get; set; } = 1.0m;

        [Configuration]
        public decimal Size { get; set; } = 1.0m;

        [Configuration]
        public List<IconCondition> Conditions { get; set; } = new List<IconCondition> 
        {
            new IconCondition
            {
                Icon = "question-circle"
            } 
        };

        public class IconCondition
        {
            public string? Icon { get; set; } = "question-circle";

            public decimal Opacity { get; set; } = 1.0m;

            public string? ThingId { get; set; }

            public string? Property { get; set; }

            public string? Value { get; set; }

            public bool IsActive(IThingManager thingManager)
            {
                if (ThingId == null)
                {
                    return true;
                }

                if (Property != null)
                {
                    var propertyState = thingManager.TryGetPropertyState(ThingId, Property, true);
                    return propertyState?.Value?.ToString() == Value;
                }

                return false;
            }
        }
    }
}

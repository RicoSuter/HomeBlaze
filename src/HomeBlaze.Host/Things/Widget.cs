using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using Microsoft.AspNetCore.Components;
using System;
using System.Reflection;

namespace HomeBlaze.Things
{
    public class Widget : IThing
    {
        public string? Title => $"{Thing?.GetType().Name} ({X},{Y})";

        [Configuration(IsIdentifier = true)]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public int X { get; set; }

        [Configuration]
        public int Y { get; set; }

        [Configuration, State]
        public IThing? Thing { get; set; }

        // TODO: Cache this
        public bool NoPointerEvents => Thing?.GetType()?.GetCustomAttribute<ThingWidgetAttribute>()?.NoPointerEvents == true;

        public RenderFragment RenderFragment
        {
            get
            {
                return builder =>
                {
                    var widgetType = Thing?.GetType()?.GetCustomAttribute<ThingWidgetAttribute>()?.ComponentType;
                    if (widgetType != null)
                    {
                        builder.OpenComponent(0, widgetType);
                        builder.AddAttribute(1, "Thing", Thing);
                        builder.CloseComponent();
                    }
                    else
                    {
                        builder.AddContent(0, $"{nameof(ThingWidgetAttribute)} missing.");
                    }
                };
            }
        }
    }
}

using Microsoft.AspNetCore.Components;
using System;

namespace HomeBlaze.Components.Abstractions
{
    public interface IControlComponentProvider
    {
        Type? ControlComponentType { get; }

        public RenderFragment ControlComponentRenderFragment
        {
            get
            {
                return builder =>
                {
                    var controlViewType = ControlComponentType;
                    if (controlViewType != null)
                    {
                        builder.OpenComponent(0, controlViewType);
                        builder.AddAttribute(1, "Thing", this);
                        builder.CloseComponent();
                    }
                };
            }
        }
    }
}

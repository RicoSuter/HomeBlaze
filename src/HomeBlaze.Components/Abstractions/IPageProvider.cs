using HomeBlaze.Abstractions;
using Microsoft.AspNetCore.Components;
using System;

namespace HomeBlaze.Components.Abstractions
{
    public interface IPageProvider : IThing
    {
        string? PageTitle { get; }

        Type? PageComponentType { get; }

        public RenderFragment PageRenderFragment
        {
            get
            {
                return builder =>
                {
                    var controlViewType = PageComponentType;
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

using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using HomeBlaze.Things;

namespace HomeBlaze.Host.Pages
{
    public class DashboardWidgetNodeModel : NodeModel
    {
        public Widget Widget { get; }

        public DashboardWidgetNodeModel(Widget widget)
        {
            Position = new Point(widget.X, widget.Y);
            Widget = widget;
            Moving += model => UpdatePosition();
        }

        public void UpdatePosition()
        {
            Widget.X = (int)(Position?.X ?? 0);
            Widget.Y = (int)(Position?.Y ?? 0);
        }
    }
}

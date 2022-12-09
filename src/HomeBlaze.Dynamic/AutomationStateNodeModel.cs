using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;

namespace HomeBlaze.Dynamic
{
    public class AutomationStateNodeModel : NodeModel
    {
        public AutomationState State { get; }

        public AutomationStateNodeModel(AutomationState state)
        {
            State = state;
            
            Position = new Point(state.X, state.Y);
            Moving += model =>
            {
                State.X = (int)(model.Position?.X ?? 0);
                State.Y = (int)(model.Position?.Y ?? 0);
            };

            AddPort(PortAlignment.Bottom);
            AddPort(PortAlignment.Top);
            AddPort(PortAlignment.Left);
            AddPort(PortAlignment.Right);
        }

        public override void Refresh()
        {
            Title = State.ToString();
            base.Refresh();
        }
    }
}

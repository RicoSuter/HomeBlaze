using Blazor.Diagrams.Core.Models;
using MudBlazor;

namespace HomeBlaze.Dynamic
{
    public class AutomationTransitionLinkModel : LinkModel
    {
        public AutomationTransition Transition { get; }

        public AutomationTransitionLinkModel(AutomationTransition transition, PortModel sourcePort, PortModel? targetPort = null)
            : base(sourcePort, targetPort)
        {
            TargetMarker = LinkMarker.Arrow;
            Transition = transition;

            Labels.Add(new LinkLabelModel(this, transition.Title));

            Changed += () =>
            {
                Transition.FromState = (SourceNode as AutomationStateNodeModel)?.State.Name;
                Transition.FromPort = SourcePort?.Alignment;

                Transition.ToState = (TargetNode as AutomationStateNodeModel)?.State.Name;
                Transition.ToPort = TargetPort?.Alignment;
            };
        }

        public override void Refresh()
        {
            Labels[0].Content = Transition.Title;
            base.Refresh();
        }
    }
}

using Blazor.Diagrams.Core.Models;

namespace HomeBlaze.Dynamic
{
    public class AutomationTransitionLinkModel : LinkModel
    {
        public AutomationTransition Transition { get; }

        public AutomationTransitionLinkModel(
            AutomationTransition transition, PortModel sourcePort, PortModel targetPort)
            : base(sourcePort, targetPort)
        {
            TargetMarker = LinkMarker.Arrow;
            Transition = transition;

            Labels.Add(new LinkLabelModel(this, transition.Title));

            Changed += (m) =>
            {
                Transition.FromState = (Source.Model as AutomationStateNodeModel)?.State.Name;
                //Transition.FromPort = Source?.;

                Transition.ToState = (Target.Model as AutomationStateNodeModel)?.State.Name;
                //Transition.ToPort = this.Target?.Alignment;
            };
        }

        public override void Refresh()
        {
            Labels[0].Content = Transition.Title;
            base.Refresh();
        }
    }
}

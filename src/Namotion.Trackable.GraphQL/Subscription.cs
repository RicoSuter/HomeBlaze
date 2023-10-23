namespace Namotion.Trackable.GraphQL
{
    public class Subscription<TTrackable>
    {
        [Subscribe]
        public TTrackable Root([EventMessage] TTrackable trackable) => trackable;
    }
}
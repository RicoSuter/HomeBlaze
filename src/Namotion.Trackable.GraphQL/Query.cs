namespace Namotion.Trackable.GraphQL
{
    public class Query<TTrackable>
    {
        private readonly TTrackable _trackable;

        public Query(TTrackable trackable)
        {
            _trackable = trackable;
        }

        public TTrackable GetRoot() => _trackable;
    }
}
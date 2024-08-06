namespace Namotion.Proxy.GraphQL
{
    public class Query<TProxy>
    {
        private readonly TProxy _proxy;

        public Query(TProxy proxy)
        {
            _proxy = proxy;
        }

        public TProxy GetRoot() => _proxy;
    }
}
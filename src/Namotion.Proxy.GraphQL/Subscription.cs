namespace Namotion.Proxy.GraphQL
{
    public class Subscription<TProxy>
    {
        [Subscribe]
        public TProxy Root([EventMessage] TProxy proxy) => proxy;
    }
}
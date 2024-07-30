using Namotion.Proxy.Abstractions;

namespace Namotion.Proxy.Tests
{
    public class TestProxyPropertyRegistryHandler : IProxyLifecycleHandler
    {
        private readonly List<ProxyLifecycleContext> _attaches;
        private readonly List<ProxyLifecycleContext> _detaches;

        public TestProxyPropertyRegistryHandler(
            List<ProxyLifecycleContext> attaches,
            List<ProxyLifecycleContext> detaches)
        {
            _attaches = attaches;
            _detaches = detaches;
        }

        public void OnProxyAttached(ProxyLifecycleContext context)
        {
            _attaches.Add(context);
        }

        public void OnProxyDetached(ProxyLifecycleContext context)
        {
            _detaches.Add(context);
        }
    }
}

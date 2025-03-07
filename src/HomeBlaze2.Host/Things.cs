using System.Collections.Concurrent;
using Namotion.Proxy;
using Namotion.Proxy.Sources.Attributes;

namespace HomeBlaze2.Host
{
    [GenerateProxy]
    public partial class Things
    {
        [ProxySource("mqtt", "name")]
        [ProxySource("opc", "Name")] 
        public partial string Name { get; set; }
        
        private readonly ConcurrentDictionary<string, IProxy> _allThings = new();

        public IReadOnlyDictionary<string, IProxy> AllThings => _allThings.AsReadOnly();

        public void AddThing(string name, IProxy proxy)
        {
            _allThings.AddOrUpdate(name, proxy, (key, existing) => proxy);
        }
    }
}
using System.Text.Json.Serialization;

namespace Namotion.Proxy.Registry.Abstractions;

public record RegisteredProxy
{
    private readonly object _lock = new();

    private readonly Dictionary<string, RegisteredProxyProperty> _properties;
    private readonly HashSet<ProxyPropertyReference> _parents = new();

    [JsonIgnore]
    public IProxy Proxy { get; }

    public ICollection<ProxyPropertyReference> Parents
    {
        get
        {
            lock (_lock)
                return _parents.ToArray();
        }
    }

    public IReadOnlyDictionary<string, RegisteredProxyProperty> Properties
    {
        get
        {
            lock (_lock)
                return _properties!.ToDictionary(p => p.Key, p => p.Value);
        }
    }

    internal RegisteredProxy(IProxy proxy, IEnumerable<RegisteredProxyProperty> properties)
    {
        Proxy = proxy;
        _properties = properties
            .ToDictionary(
                p => p.Property.Name,
                p =>
                {
                    p.Parent = this;
                    return p;
                });
    }

    public void AddParent(ProxyPropertyReference parent)
    {
        lock (_lock)
            _parents.Add(parent);
    }

    public void RemoveParent(ProxyPropertyReference parent)
    {
        lock (_lock)
            _parents.Remove(parent);
    }

    public void AddProperty(string name, Type type, Func<object?>? getValue, Action<object?>? setValue, params object[] attributes)
    {
        lock (_lock)
        {
            _properties!.Add(name, new CustomRegisteredProxyProperty(new ProxyPropertyReference(Proxy, name), getValue, setValue)
            {
                Parent = this,
                Type = type,
                Attributes = attributes
            });
        }
    }
}

using Namotion.Proxy.Attributes;

namespace Namotion.Proxy.Registry.Abstractions;

#pragma warning disable CS8618
public record RegisteredProxyProperty(ProxyPropertyReference Property)
#pragma warning restore CS8618
{
    private readonly HashSet<ProxyPropertyChild> _children = new();

    public required Type Type { get; init; }

    public required object[] Attributes { get; init; }

    public RegisteredProxy Parent { get; internal set; }

    public virtual bool HasGetter => Property.Metadata.GetValue is not null;

    public virtual bool HasSetter => Property.Metadata.SetValue is not null;

    public virtual object? GetValue()
    {
        return Property.Metadata.GetValue?.Invoke(Property.Proxy);
    }

    public virtual void SetValue(object? value)
    {
        Property.Metadata.SetValue?.Invoke(Property.Proxy, value);
    }

    public ICollection<ProxyPropertyChild> Children
    {
        get
        {
            lock (this)
            {
                return _children.ToArray();
            }
        }
    }

    public void AddChild(ProxyPropertyChild parent)
    {
        lock (this)
            _children.Add(parent);
    }

    public void RemoveChild(ProxyPropertyChild parent)
    {
        lock (this)
            _children.Remove(parent);
    }

    public void AddAttribute(string name, Type type, Func<object?>? getValue, Action<object?>? setValue)
    {
        Parent.AddProperty(
            $"{Property.Name}_{name}",
            type, getValue, setValue,
            new PropertyAttributeAttribute(Property.Name, name));
    }
}

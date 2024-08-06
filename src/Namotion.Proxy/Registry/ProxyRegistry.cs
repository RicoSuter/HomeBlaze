using Namotion.Proxy.Abstractions;
using Namotion.Proxy.Registry.Abstractions;
using System.Collections.Immutable;

namespace Namotion.Proxy.Registry;

// TODO: Add lots of tests!

internal class ProxyRegistry : IProxyRegistry, IProxyLifecycleHandler
{
    private Dictionary<IProxy, RegisteredProxy> _knownProxies = new();

    public IReadOnlyDictionary<IProxy, RegisteredProxy> KnownProxies
    {
        get
        {
            lock (_knownProxies)
                return _knownProxies.ToImmutableDictionary();
        }
    }

    public void OnProxyAttached(ProxyLifecycleContext context)
    {
        lock (_knownProxies)
        {
            if (!_knownProxies.TryGetValue(context.Proxy, out var metadata))
            {
                metadata = new RegisteredProxy(context.Proxy, context.Proxy
                    .Properties
                    .Select(p => new RegisteredProxyProperty(new ProxyPropertyReference(context.Proxy, p.Key))
                    {
                        Type = p.Value.Type,
                        Attributes = p.Value.Attributes
                    }));

                _knownProxies[context.Proxy] = metadata;
            }

            if (context.Property != default)
            {
                metadata.AddParent(context.Property);

                _knownProxies
                    .TryGetProperty(context.Property)?
                    .AddChild(new ProxyPropertyChild
                    {
                        Proxy = context.Proxy,
                        Index = context.Index
                    });
            }

            foreach (var property in metadata.Properties)
            {
                foreach (var attribute in property.Value.Attributes.OfType<IProxyPropertyInitializer>())
                {
                    attribute.InitializeProperty(property.Value, context.Index, context.Context);
                }
            }
        }
    }

    public void OnProxyDetached(ProxyLifecycleContext context)
    {
        lock (_knownProxies)
        {
            if (context.ReferenceCount == 0)
            {
                if (context.Property != default)
                {
                    var metadata = _knownProxies[context.Proxy];
                    metadata.RemoveParent(context.Property);

                    _knownProxies
                        .TryGetProperty(context.Property)?
                        .RemoveChild(new ProxyPropertyChild
                        {
                            Proxy = context.Proxy,
                            Index = context.Index
                        });
                }

                _knownProxies.Remove(context.Proxy);
            }
        }
    }
}

using Namotion.Proxy.Abstractions;

using System.Collections;

namespace Namotion.Proxy.ChangeTracking;

internal class ProxyLifecycleHandler : IProxyWriteHandler, IProxyLifecycleHandler
{
    private const string ReferenceCountKey = "Namotion.ReferenceCount";
 
    private readonly Lazy<IProxyLifecycleHandler[]> _handlers;

    public ProxyLifecycleHandler(Lazy<IProxyLifecycleHandler[]> handlers)
    {
        _handlers = handlers;
    }

    // TODO: does it make sense that the two methods are not "the same"?

    public void OnProxyAttached(ProxyLifecycleContext context)
    {
        var touchedProxies = new HashSet<IProxy>();
        var proxyProperties = new HashSet<(IProxy, ProxyPropertyReference, object?)>();
        FindProxiesInProperties(context.Proxy, touchedProxies, proxyProperties);
        
        foreach (var child in proxyProperties)
        {
            AttachProxy(context.Context, child.Item2, child.Item1, child.Item3);
        }
    }

    // TODO: What should we do here?
    public void OnProxyDetached(ProxyLifecycleContext context)
    {
        //foreach (var child in FindProxiesInProperties(context.Proxy, new HashSet<IProxy>()))
        //{
        //    DetachProxy(context.Context, child.Item1, child.Item2, child.Item3);
        //}

        //DetachProxy(context.Context, context.Property, context., child.Item3);
    }

    public void WriteProperty(ProxyPropertyWriteContext context, Action<ProxyPropertyWriteContext> next)
    {
        var currentValue = context.CurrentValue;
        next(context);
        var newValue = context.NewValue;

        if (!Equals(currentValue, newValue))
        {
            var oldProxies = new HashSet<IProxy>();
            var oldProxyProperties = new HashSet<(IProxy, ProxyPropertyReference, object?)>();
            FindProxies(context.Property, currentValue, null, oldProxies, oldProxyProperties);

            var newProxies = new HashSet<IProxy>();
            var newProxyProperties = new HashSet<(IProxy, ProxyPropertyReference, object?)>();
            FindProxies(context.Property, newValue, null, newProxies, newProxyProperties);

            if (oldProxyProperties.Count != 0 || newProxyProperties.Count != 0)
            {
                foreach (var d in oldProxyProperties
                    .Where(u => !newProxies.Contains(u.Item1)))
                {
                    DetachProxy(context.Context, d.Item2, d.Item1, d.Item3);
                }

                foreach (var d in newProxyProperties
                    .Where(u => !oldProxies.Contains(u.Item1)))
                {
                    AttachProxy(context.Context, d.Item2, d.Item1, d.Item3);
                }
            }
        }
    }

    private void AttachProxy(IProxyContext context, ProxyPropertyReference property, IProxy proxy, object? index)
    {
        var count = proxy.Data.AddOrUpdate(ReferenceCountKey, 1, (_, count) => (int)count! + 1) as int?;
        var registryContext = new ProxyLifecycleContext(property, index, proxy, count ?? 1, context);

        foreach (var handler in _handlers.Value)
        {
            if (handler != this)
            {
                handler.OnProxyAttached(registryContext);
            }
        }
    }

    private void DetachProxy(IProxyContext context, ProxyPropertyReference property, IProxy proxy, object? index)
    {
        var count = proxy.Data.AddOrUpdate(ReferenceCountKey, 0, (_, count) => (int)count! - 1) as int?;
        var registryContext = new ProxyLifecycleContext(property, index, proxy, count ?? 1, context);
       
        foreach (var handler in _handlers.Value)
        {
            if (handler != this)
            {
                handler.OnProxyDetached(registryContext);
            }
        }
    }

    private void FindProxies(
        ProxyPropertyReference property,
        object? value, object? index,
        HashSet<IProxy> touchedProxies,
        HashSet<(IProxy, ProxyPropertyReference, object?)> result)
    {
        if (value is IDictionary dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                var dictionaryValue = dictionary[key];
                if (dictionaryValue is IProxy proxy)
                {
                    FindProxies(property, proxy, key, touchedProxies, result);
                }
            }
        }
        else if (value is ICollection collection)
        {
            var i = 0;
            foreach (var proxy in collection.OfType<IProxy>())
            {
                FindProxies(property, proxy, i, touchedProxies, result);
                i++;
            }
        }
        else if (value is IProxy proxy)
        {
            result.Add((proxy, property, index));

            if (!touchedProxies.Contains(proxy))
            {
                touchedProxies.Add(proxy);
                FindProxiesInProperties(proxy, touchedProxies, result);
            }
        }
    }

    private void FindProxiesInProperties(IProxy proxy,
        HashSet<IProxy> touchedProxies,
        HashSet<(IProxy, ProxyPropertyReference, object?)> result)
    {
        foreach (var property in proxy.Properties)
        {
            var childValue = property.Value.GetValue?.Invoke(proxy);
            if (childValue is not null)
            {
                FindProxies(new ProxyPropertyReference(proxy, property.Key), childValue, null, touchedProxies, result);
            }
        }
    }
}
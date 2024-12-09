using Namotion.Proxy.Abstractions;

namespace Namotion.Proxy.ChangeTracking;

/// <summary>
/// Should be used with <see cref="InitiallyLoadDerivedPropertiesHandler"/> so that dependencies are initially set up.
/// </summary>
internal class DerivedPropertyChangeDetectionHandler : IProxyLifecycleHandler, IProxyReadHandler, IProxyWriteHandler
{
    private readonly Lazy<IProxyWriteHandler[]> _handlers;

    [ThreadStatic]
    private static Stack<HashSet<ProxyPropertyReference>>? _currentTouchedProperties;

    public DerivedPropertyChangeDetectionHandler(Lazy<IProxyWriteHandler[]> handlers)
    {
        _handlers = handlers;
    }

    public void OnProxyAttached(ProxyLifecycleContext context)
    {
        foreach (var property in context.Proxy.Properties.Where(p => p.Value.IsDerived))
        {
            var propertyReference = new ProxyPropertyReference(context.Proxy, property.Key);

            TryStartRecordTouchedProperties();

            var result = property.Value.GetValue?.Invoke(context.Proxy);
            propertyReference.SetLastKnownValue(result);

            StoreRecordedTouchedProperties(propertyReference);
            TouchProperty(propertyReference);
        }
    }

    public void OnProxyDetached(ProxyLifecycleContext context)
    {
    }

    public object? ReadProperty(ProxyPropertyReadContext context, Func<ProxyPropertyReadContext, object?> next)
    {
        var result = next(context);
        TouchProperty(context.Property);
        return result;
    }

    public void WriteProperty(ProxyPropertyWriteContext context, Action<ProxyPropertyWriteContext> next)
    {
        next.Invoke(context);

        var usedByProperties = context.Property.GetUsedByProperties();
        if (usedByProperties.Any())
        {
            lock (usedByProperties)
            {
                foreach (var usedByProperty in usedByProperties)
                {
                    var oldValue = usedByProperty.GetLastKnownValue();

                    TryStartRecordTouchedProperties();

                    var newValue = usedByProperty
                        .Proxy
                        .Properties[usedByProperty.Name]
                        .GetValue?
                        .Invoke(usedByProperty.Proxy);

                    StoreRecordedTouchedProperties(usedByProperty);
                    TouchProperty(usedByProperty);

                    usedByProperty.SetLastKnownValue(newValue);

                    var changedContext = new ProxyPropertyWriteContext(usedByProperty, oldValue, newValue, usedByProperty.Metadata.IsDerived, context.Context);
                    changedContext.CallWriteProperty(newValue, delegate { }, _handlers.Value);
                }
            }
        }
    }

    private static void TryStartRecordTouchedProperties()
    {
        if (_currentTouchedProperties == null)
        {
            _currentTouchedProperties = new Stack<HashSet<ProxyPropertyReference>>();
        }

        _currentTouchedProperties.Push([]);
    }

    private static void StoreRecordedTouchedProperties(ProxyPropertyReference property)
    {
        var newProperties = _currentTouchedProperties!.Pop();

        var previouslyRequiredProperties = property.GetRequiredProperties();
        foreach (var previouslyRequiredProperty in previouslyRequiredProperties.Except(newProperties))
        {
            var usedByProperties = previouslyRequiredProperty.GetUsedByProperties();
            lock (usedByProperties)
                usedByProperties.Remove(previouslyRequiredProperty);
        }

        property.SetRequiredProperties(newProperties);

        foreach (var newlyRequiredProperty in newProperties.Except(previouslyRequiredProperties))
        {
            var usedByProperties = newlyRequiredProperty.GetUsedByProperties();
            lock (usedByProperties)
                usedByProperties.Add(new ProxyPropertyReference(property.Proxy, property.Name));
        }
    }

    private static void TouchProperty(ProxyPropertyReference property)
    {
        if (_currentTouchedProperties?.TryPeek(out var touchedProperties) == true)
        {
            touchedProperties.Add(property);
        }
        else
        {
            _currentTouchedProperties = null;
        }
    }
}

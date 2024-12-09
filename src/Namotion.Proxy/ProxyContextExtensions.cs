using Namotion.Proxy.Abstractions;
using Namotion.Proxy.ChangeTracking;

namespace Namotion.Proxy;

public static class ProxyContextExtensions
{
    public static THandler GetHandler<THandler>(this IProxyContext context)
        where THandler : IProxyHandler
    {
        return context.GetHandlers<THandler>().Single();
    }

    public static IObservable<ProxyPropertyChanged> GetPropertyChangedObservable(this IProxyContext context)
    {
        return context.GetHandlers<PropertyChangedObservable>().Single();
    }

    public static void CallWriteProperty(this ProxyPropertyWriteContext context, object? newValue, Action<object?> writeValue, IProxyWriteHandler[] writeHandlers)
    {
        for (int i = 0; i < writeHandlers.Length; i++)
        {
            var handler = writeHandlers[i];
            var previousWriteValue = writeValue;
            writeValue = (value) =>
            {
                handler.WriteProperty(context with { NewValue = value }, ctx => previousWriteValue(ctx.NewValue));
            };
        }

        writeValue(newValue);
    }
}

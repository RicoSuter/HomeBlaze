using Namotion.Proxy.Abstractions;
using System.Reactive.Subjects;

namespace Namotion.Proxy.ChangeTracking;

internal class PropertyChangedObservable : IObservable<ProxyPropertyChanged>, IProxyWriteHandler
{
    private Subject<ProxyPropertyChanged> _subject = new();

    public void WriteProperty(ProxyPropertyWriteContext context, Action<ProxyPropertyWriteContext> next)
    {
        var currentValue = context.CurrentValue;
        var newValue = context.NewValue;

        next(context); 
        
        // TODO: Should retrieve actual new value

        var changedContext = new ProxyPropertyChanged(context.Property, currentValue, newValue, context.Context);
        _subject.OnNext(changedContext);
    }

    public IDisposable Subscribe(IObserver<ProxyPropertyChanged> observer)
    {
        return _subject.Subscribe(observer);
    }
}

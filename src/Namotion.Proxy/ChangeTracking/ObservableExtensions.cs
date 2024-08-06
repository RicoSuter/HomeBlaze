using Namotion.Proxy.Abstractions;

using System.Reactive.Linq;

namespace Namotion.Proxy.ChangeTracking;

public static class ObservableExtensions
{
    public static IObservable<IEnumerable<ProxyPropertyChanged>> BufferChanges(this IObservable<ProxyPropertyChanged> observable, TimeSpan bufferTime)
    {
        return observable
            .Buffer(bufferTime)
            .Where(propertyChanges => propertyChanges.Any())
            .Select(propertyChanges => propertyChanges.Reverse().DistinctBy(c => c.Property));
    }
}

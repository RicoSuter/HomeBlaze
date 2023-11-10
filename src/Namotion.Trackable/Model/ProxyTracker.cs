namespace Namotion.Trackable.Model;

public class ProxyTracker : Tracker
{
    public ProxyTracker(ITrackable proxy, TrackedProperty? parentProperty, object? parentCollectionKey)
        : base(parentProperty, parentCollectionKey)
    {
        Object = proxy;
    }

    public ITrackable Object { get; }
}

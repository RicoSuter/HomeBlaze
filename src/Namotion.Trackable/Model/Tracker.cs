using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Namotion.Trackable.Model;

public class Tracker
{
    public Tracker(ITrackable proxy, string path, TrackedProperty? parentProperty, ITrackableContext context, ITrackableFactory factory)
    {
        Object = proxy;
        Path = path;
        ParentProperty = parentProperty;
        Context = context;
        Factory = factory;
    }

    public ITrackable Object { get; }

    public string Path { get; }

    public TrackedProperty? ParentProperty { get; }

    public ICollection<TrackedProperty> Properties { get; } = new HashSet<TrackedProperty>();

    public ITrackableContext Context { get; }

    public ITrackableFactory Factory { get; }

    [JsonIgnore]
    public IDictionary<string, object?> Data { get; } = new Dictionary<string, object?>();
}

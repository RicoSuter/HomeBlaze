using System.Collections.Generic;

namespace Namotion.Trackable.Model;

public class Tracker
{
    public Tracker(object thing, string path, TrackedProperty? parentProperty, ITrackableContext context)
    {
        Object = thing;
        Path = path;
        ParentProperty = parentProperty;
        Context = context;
    }

    public object Object { get; }

    public string Path { get; }

    public TrackedProperty? ParentProperty { get; }

    public ICollection<TrackedProperty> Properties { get; } = new HashSet<TrackedProperty>();

    public ITrackableContext Context { get; }
}

using System.Collections.Generic;

namespace Namotion.Trackable.Model;

public class Tracker
{
    public Tracker(object thing, string path, TrackedProperty? parentProperty)
    {
        Object = thing;
        Path = path;
        ParentProperty = parentProperty;
    }

    public object Object { get; }

    public string Path { get; }

    public TrackedProperty? ParentProperty { get; }

    public ICollection<TrackedProperty> Properties { get; } = new HashSet<TrackedProperty>();
}

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Namotion.Trackable.Model;

public class Trackable
{
    public Trackable(object thing, string path, TrackableProperty? parent, int? parentCollectionIndex)
    {
        Object = thing;
        Path = path;
        Parent = parent;
        ParentCollectionIndex = parentCollectionIndex;
    }

    public object Object { get; }

    public string Path { get; }

    public TrackableProperty? Parent { get; }

    public int? ParentCollectionIndex { get; }

    public ICollection<TrackableProperty> Properties { get; } = new HashSet<TrackableProperty>();
}

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Namotion.Trackable.Model;

public class Trackable
{
    public Trackable(object thing, string path, TrackableProperty? parent)
    {
        Object = thing;
        Path = path;
        Parent = parent;
    }

    public object Object { get; }

    public string Path { get; }

    public TrackableProperty? Parent { get; }

    public ICollection<TrackableProperty> Properties { get; } = new HashSet<TrackableProperty>();

    [JsonExtensionData]
    public IDictionary<string, object?> ExtensionData { get; } = new Dictionary<string, object?>();
}

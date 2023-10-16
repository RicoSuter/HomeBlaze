using System.Collections.Generic;

namespace Namotion.Trackable.Model;

public class Trackable
{
    public Trackable(object thing, string path, string? sourcePath, Trackable? parent)
    {
        Object = thing;
        Path = path;
        SourcePath = sourcePath;
        Parent = parent;
    }

    public object Object { get; }

    public string Path { get; }

    public string? SourcePath { get; }

    public Trackable? Parent { get; }

    public ICollection<TrackableProperty> Properties { get; } = new HashSet<TrackableProperty>();
}

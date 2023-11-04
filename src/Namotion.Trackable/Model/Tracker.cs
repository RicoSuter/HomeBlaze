using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Namotion.Trackable.Model;

public class Tracker
{
    private ConcurrentDictionary<string, TrackedProperty> _properties = new ConcurrentDictionary<string, TrackedProperty>();

    public Tracker(ITrackable proxy, string path, TrackedProperty? parentProperty, ITrackableContext context)
    {
        Object = proxy;
        Path = path;
        ParentProperty = parentProperty;
        Context = context;
    }

    public ITrackable Object { get; }

    public string Path { get; }

    public TrackedProperty? ParentProperty { get; }

    public IReadOnlyDictionary<string, TrackedProperty> Properties => _properties;

    public ITrackableContext Context { get; }

    [JsonIgnore]
    public IDictionary<string, object?> Data { get; } = new Dictionary<string, object?>();

    public TrackedProperty? TryGetProperty(string propertyName)
    {
        return Properties.TryGetValue(propertyName, out var property) ? property : null;
    }

    internal void AddProperty(TrackedProperty property)
    {
        _properties[property.PropertyName] = property;
    }
}

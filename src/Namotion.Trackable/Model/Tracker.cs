using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Namotion.Trackable.Model;

public class Tracker
{
    private IDictionary<string, TrackedProperty> _properties = new ConcurrentDictionary<string, TrackedProperty>();

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

    public IEnumerable<TrackedProperty> Properties => _properties.Values;

    public ITrackableContext Context { get; }

    [JsonIgnore]
    public IDictionary<string, object?> Data { get; } = new Dictionary<string, object?>();

    public TrackedProperty? TryGetProperty(string propertyName)
    {
        return _properties.TryGetValue(propertyName, out var property) ? property : null;
    }

    internal void AddProperty(TrackedProperty property)
    {
        _properties[property.PropertyName] = property;
    }

    internal void FreezeProperties()
    {
        _properties = new ReadOnlyDictionary<string, TrackedProperty>(_properties);
    }
}

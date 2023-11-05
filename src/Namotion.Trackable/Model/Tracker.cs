using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Namotion.Trackable.Model;

public class Tracker
{
    private IDictionary<string, TrackedProperty> _properties = new ConcurrentDictionary<string, TrackedProperty>();

    private string? _path;

    public Tracker(ITrackable proxy, TrackedProperty? parentProperty, object? parentCollectionKey)
    {
        Object = proxy;
        ParentProperty = parentProperty;
        ParentCollectionKey = parentCollectionKey;
    }

    public ITrackable Object { get; }

    public string Path => _path ??= (ParentProperty != null ?
        (ParentProperty.Path + (ParentCollectionKey != null ? $"[{ParentCollectionKey}]" : string.Empty)) :
        string.Empty);

    public TrackedProperty? ParentProperty { get; }

    public object? ParentCollectionKey { get; }

    public IEnumerable<TrackedProperty> Properties => _properties.Values;

    [JsonIgnore]
    public IDictionary<string, object?> Data { get; } = new Dictionary<string, object?>();

    public TrackedProperty? TryGetProperty(string propertyName)
    {
        return _properties.TryGetValue(propertyName, out var property) ? property : null;
    }

    public void AddProperty(TrackedProperty property)
    {
        _properties[property.Name] = property;
    }

    internal void FreezeProperties()
    {
        _properties = new ReadOnlyDictionary<string, TrackedProperty>(_properties);
    }
}

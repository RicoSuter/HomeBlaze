using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Namotion.Trackable.Model
{
    public class Tracker
    {
        private string? _path;
        private IDictionary<string, TrackedProperty> _properties = new ConcurrentDictionary<string, TrackedProperty>();

        [JsonIgnore]
        public IDictionary<string, object?> Data { get; } = new Dictionary<string, object?>();

        public object? ParentCollectionKey { get; }

        public TrackedProperty? ParentProperty { get; }

        public string Path => _path ??= (ParentProperty != null ?
            (ParentProperty.Path + (ParentCollectionKey != null ? $"[{ParentCollectionKey}]" : string.Empty)) :
            string.Empty);

        public IReadOnlyDictionary<string, TrackedProperty> Properties => _properties.AsReadOnly();

        public Tracker()
        {
        }

        public Tracker(TrackedProperty? parentProperty, object? parentCollectionKey)
            : this()
        {
            ParentProperty = parentProperty;
            ParentCollectionKey = parentCollectionKey;
        }

        public void AddProperty(TrackedProperty property)
        {
            _properties[property.Name] = property;
        }

        public TrackedProperty? TryGetProperty(string propertyName)
        {
            return _properties.TryGetValue(propertyName, out var property) ? property : null;
        }

        internal void FreezeProperties()
        {
            _properties = new ReadOnlyDictionary<string, TrackedProperty>(_properties);
        }
    }
}
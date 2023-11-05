using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

namespace Namotion.Trackable.Model;

public abstract class TrackedProperty
{
    public TrackedProperty(string name, Tracker parent)
    {
        Name = name;
        Parent = parent;
        Path = !string.IsNullOrEmpty(parent.Path) ? $"{parent.Path}.{name}" : name;
    }

    [JsonIgnore]
    public string Name { get; protected set; }

    /// <summary>
    /// Gets the full property path with the trackable context object as root.
    /// </summary>
    public string Path { get; }

    public abstract bool IsReadable { get; }

    public abstract bool IsWriteable { get; }

    [MemberNotNullWhen(true, nameof(AttributedProperty))]
    [MemberNotNullWhen(true, nameof(AttributeName))]
    public bool IsAttribute => AttributedProperty != null;

    [JsonIgnore]
    public Tracker Parent { get; }

    public abstract bool IsDerived { get; }

    [JsonIgnore]
    public abstract Type PropertyType { get; }

    [JsonIgnore]
    public TrackedProperty? AttributedProperty { get; private set; }

    [JsonIgnore]
    public string? AttributeName { get; private set; }

    [JsonIgnore]
    public IEnumerable<TrackedProperty>? DependentProperties { get; internal set; }

    public IEnumerable<string> DependentPropertyPaths => DependentProperties?.Select(v => v.Path) ?? Array.Empty<string>();

    public Dictionary<string, TrackedProperty> Attributes => Parent
        .Properties
        .Where(p => p.AttributedProperty == this)
        .ToDictionary(v => v.AttributeName!, v => v);

    [JsonIgnore]
    public IDictionary<string, object?> Data { get; } = new Dictionary<string, object?>();

    /// <summary>
    /// Gets the last known value of this property.
    /// </summary>
    public object? LastValue { get; internal set; }

    public void ToAttribute(string attributeName, string propertyName)
    {
        AttributeName = attributeName;
        AttributedProperty = Parent.TryGetProperty(propertyName) ??
            throw new InvalidOperationException($"Cannot find property {propertyName}.");
    }

    public virtual object? GetValue()
    {
        throw new NotImplementedException();
    }

    public virtual void SetValue(object? value)
    {
        throw new NotImplementedException();
    }
}

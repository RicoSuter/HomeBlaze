using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Namotion.Trackable.Model;

public class TrackedProperty
{
    private MethodInfo? _getMethod;
    private MethodInfo? _setMethod;

    private readonly PropertyInfo _property;

    public TrackedProperty(
        PropertyInfo property,
        string path,
        Tracker parent)
    {
        _property = property;

        Path = path;
        Parent = parent;

        _getMethod = property.GetMethod;
        _setMethod = property.SetMethod;
    }

    [JsonIgnore]
    public string Name => _property.Name;

    /// <summary>
    /// Gets the full property path with the trackable context object as root.
    /// </summary>
    public string Path { get; }

    public bool IsReadable => _getMethod != null;

    public bool IsWriteable => _setMethod != null;

    [MemberNotNullWhen(true, nameof(AttributedProperty))]
    [MemberNotNullWhen(true, nameof(AttributeName))]
    public bool IsAttribute => AttributedProperty != null;

    [JsonIgnore]
    public Tracker Parent { get; }

    public bool IsDerived => _setMethod == null;

    [JsonIgnore]
    public Type PropertyType => _property.PropertyType;

    [JsonIgnore]
    public TrackedProperty? AttributedProperty { get; set; }

    [JsonIgnore]
    public string? AttributeName { get; set; }

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

    public object? GetValue()
    {
        return _property.GetValue(Parent.Object);
    }

    public void SetValue(object? value)
    {
        _property.SetValue(Parent.Object, value);
    }
}

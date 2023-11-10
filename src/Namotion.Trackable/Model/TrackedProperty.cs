using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

namespace Namotion.Trackable.Model;

public abstract class TrackedProperty
{
    private readonly IObserver<TrackedPropertyChange> _observer;

    private string? _path;

    public TrackedProperty(string name, Tracker parent, IObserver<TrackedPropertyChange> observer)
    {
        _observer = observer;

        Name = name;
        Parent = parent;
    }

    [JsonIgnore]
    public string Name { get; protected set; }

    /// <summary>
    /// Gets the full property path with the trackable context object as root.
    /// </summary>
    public string Path => _path ??= !string.IsNullOrEmpty(Parent.Path) ? $"{Parent.Path}.{Name}" : Name;

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

    /// <summary>
    /// Gets the properties which are used to calculate the value of this derived property.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<TrackedProperty> RequiredProperties { get; internal set; } = ImmutableHashSet<TrackedProperty>.Empty;

    // TODO: Make all these immutable below

    /// <summary>
    /// Gets the properties which use this property.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<TrackedProperty> UsedByProperties { get; private set; } = new HashSet<TrackedProperty>();

    [JsonIgnore]
    public IReadOnlyCollection<ProxyTracker> Children { get; internal set; } = new HashSet<ProxyTracker>();

    internal IEnumerable<string> DependentPropertyPaths => RequiredProperties?.Select(v => v.Path) ?? Array.Empty<string>();

    /// <summary>
    /// Gets the attributes of this property which are internally properties on the same parent tracker.
    /// </summary>
    public Dictionary<string, TrackedProperty> Attributes => Parent
        .Properties
        .Where(p => p.AttributedProperty == this)
        .ToDictionary(v => v.AttributeName!, v => v);

    /// <summary>
    /// Gets the current additional data of this property (can be used by consumer).
    /// </summary>
    [JsonIgnore]
    public IImmutableDictionary<string, object?> Data { get; set; } = ImmutableDictionary<string, object?>.Empty;

    /// <summary>
    /// Gets the last known value of this property.
    /// </summary>
    public object? LastValue { get; internal set; }

    public void ConvertToAttribute(string attributeName, string propertyName)
    {
        AttributeName = attributeName;
        AttributedProperty = Parent.TryGetProperty(propertyName) ??
            throw new InvalidOperationException($"Cannot find property {propertyName}.");
    }

    public virtual object? GetValue()
    {
        return LastValue;
    }

    public virtual void SetValue(object? value)
    {
        LastValue = value;
        RaisePropertyChanged();
    }

    internal void RaisePropertyChanged()
    {
        RaisePropertyChanged(new HashSet<TrackedProperty>());
    }

    private void RaisePropertyChanged(HashSet<TrackedProperty> markedProperties)
    {
        _observer.OnNext(new TrackedPropertyChange(this, Data, LastValue));

        markedProperties.Add(this);
        foreach (var dependentProperty in UsedByProperties)
        {
            if (!markedProperties.Contains(dependentProperty))
            {
                dependentProperty.RaisePropertyChanged(markedProperties);
            }
        }
    }
}

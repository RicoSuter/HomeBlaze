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

    [ThreadStatic]
    private static Stack<HashSet<TrackedProperty>>? _currentTouchedProperties;

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

    [JsonIgnore]
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

    public IEnumerable<string> RequiredPropertyPaths => RequiredProperties?.Select(v => v.Path) ?? Array.Empty<string>();

    // TODO: Make all these immutable below

    /// <summary>
    /// Gets the properties which use this property.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyCollection<TrackedProperty> UsedByProperties { get; private set; } = new HashSet<TrackedProperty>();

    public IEnumerable<string> UsedByPropertyPaths => UsedByProperties?.Select(v => v.Path) ?? Array.Empty<string>();

    [JsonIgnore]
    public IReadOnlyCollection<ProxyTracker> Children { get; internal set; } = new HashSet<ProxyTracker>();

    /// <summary>
    /// Gets the attributes of this property which are internally properties on the same parent tracker.
    /// </summary>
    public Dictionary<string, TrackedProperty> Attributes => Parent
        .Properties
        .Where(p => p.Value.AttributedProperty == this)
        .ToDictionary(v => v.Value.AttributeName!, v => v.Value);

    /// <summary>
    /// Gets the current additional data of this property (can be used by consumer).
    /// </summary>
    [JsonIgnore]
    public IImmutableDictionary<string, object?> Data { get; set; } = ImmutableDictionary<string, object?>.Empty;

    /// <summary>
    /// Gets the last known value without evaluating the property.
    /// </summary>
    public object? LastKnownValue { get; protected internal set; }

    public void ConvertToAttribute(string attributeName, string propertyName)
    {
        AttributeName = attributeName;
        AttributedProperty = Parent.TryGetProperty(propertyName) ??
            throw new InvalidOperationException($"Cannot find property {propertyName}.");
    }

    public virtual object? GetValue()
    {
        OnBeforeRead();
        try
        {
            return LastKnownValue;
        }
        finally
        {
            OnAfterRead();
        }
    }

    public virtual void SetValue(object? value)
    {
        LastKnownValue = value;
        RaisePropertyChanged();
    }

    internal void RaisePropertyChanged()
    {
        RaisePropertyChanged(new HashSet<TrackedProperty>());
    }

    private void RaisePropertyChanged(HashSet<TrackedProperty> markedProperties)
    {
        _observer.OnNext(new TrackedPropertyChange(this, Data, GetValue()));

        markedProperties.Add(this);
        foreach (var dependentProperty in UsedByProperties)
        {
            if (!markedProperties.Contains(dependentProperty))
            {
                dependentProperty.RaisePropertyChanged(markedProperties);
            }
        }
    }

    protected internal void OnBeforeRead()
    {
        TryStartRecordingTouchedProperties();
    }

    protected internal void OnAfterRead()
    {
        StoreTouchedProperties();
        TouchProperty();
    }

    private void TryStartRecordingTouchedProperties()
    {
        if (IsDerived)
        {
            if (_currentTouchedProperties == null)
            {
                _currentTouchedProperties = new Stack<HashSet<TrackedProperty>>();
            }

            _currentTouchedProperties.Push(new HashSet<TrackedProperty>());
        }
    }

    private void StoreTouchedProperties()
    {
        if (IsDerived)
        {
            var newProperties = _currentTouchedProperties!.Pop();
            var previouslyRequiredProperties = RequiredProperties;
            if (previouslyRequiredProperties != null)
            {
                foreach (var previouslyRequiredProperty in previouslyRequiredProperties)
                {
                    if (!newProperties.Contains(previouslyRequiredProperty))
                    {
                        lock (previouslyRequiredProperty.UsedByProperties)
                            ((HashSet<TrackedProperty>)previouslyRequiredProperty.UsedByProperties).Remove(this);
                    }
                }
            }

            RequiredProperties = newProperties;

            foreach (var newlyRequiredProperty in newProperties)
            {
                lock (newlyRequiredProperty.UsedByProperties)
                    ((HashSet<TrackedProperty>)newlyRequiredProperty.UsedByProperties).Add(this);
            }
        }
    }

    private void TouchProperty()
    {
        if (_currentTouchedProperties?.TryPeek(out var touchedProperties) == true)
        {
            touchedProperties.Add(this);
        }
        else
        {
            _currentTouchedProperties = null;
        }
    }
}

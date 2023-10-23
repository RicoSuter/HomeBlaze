﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

using Namotion.Trackable.Attributes;

namespace Namotion.Trackable.Model;

public class TrackedProperty
{
    private readonly PropertyInfo _property;

    public TrackedProperty(
        PropertyInfo property,
        string path,
        Tracker parent)
    {
        _property = property;

        Path = path;
        Parent = parent;

        GetMethod = property.GetMethod;
        SetMethod = property.SetMethod;
    }

    [JsonIgnore]
    public ITrackableContext Context => Parent.Context;

    public string Path { get; }

    [MemberNotNullWhen(true, nameof(AttributeMetadata))]
    public bool IsAttribute => AttributeMetadata != null;

    [JsonIgnore]
    public Tracker Parent { get; }

    [JsonIgnore]
    public MethodInfo? GetMethod { get; }

    [JsonIgnore]
    public MethodInfo? SetMethod { get; }

    public bool IsDerived => SetMethod == null;

    [JsonIgnore]
    public string PropertyName => _property.Name;

    [JsonIgnore]
    public Type PropertyType => _property.PropertyType;

    [JsonIgnore]
    public string AbsolutePath
    {
        get
        {
            var basePath = Parent.Path;
            return (!string.IsNullOrEmpty(basePath) ? basePath + "." : "") + _property.Name;
        }
    }

    [JsonIgnore]
    public AttributeOfTrackableAttribute? AttributeMetadata => _property
        .GetCustomAttribute<AttributeOfTrackableAttribute>(true);

    [JsonIgnore]
    public TrackedProperty[] DependentProperties { get; internal set; } = Array.Empty<TrackedProperty>();

    public IEnumerable<string> DependentPropertyPaths => DependentProperties.Select(v => v.Path);

    public Dictionary<string, TrackedProperty> Attributes => Context
        .AllProperties
        .Where(v => v.AttributeMetadata?.PropertyName == _property.Name && v.Parent == Parent)
        .ToDictionary(v => v.AttributeMetadata!.AttributeName, v => v);

    [JsonIgnore]
    public IDictionary<string, object?> Data { get; } = new Dictionary<string, object?>();

    public IEnumerable<T> GetCustomAttributes<T>(bool inherit)
    {
        return _property.GetCustomAttributes(inherit).OfType<T>();
    }

    public object? GetValue()
    {
        return _property.GetValue(Parent.Object);
    }

    public void SetValue(object? value)
    {
        _property.SetValue(Parent.Object, value);
    }
}
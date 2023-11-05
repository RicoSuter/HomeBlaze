using Namotion.Trackable.Model;
using System;
using System.Reflection;

namespace Namotion.Trackable.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class AttributeOfTrackableAttribute : TrackableAttribute
{
    public AttributeOfTrackableAttribute(string propertyName, string attributeName)
    {
        PropertyName = propertyName;
        AttributeName = attributeName;
    }

    public string PropertyName { get; }

    public string AttributeName { get; }

    public override TrackedProperty CreateProperty(PropertyInfo propertyInfo, Tracker parent)
    {
        var property = new ReflectionTrackedProperty($"{PropertyName}.{AttributeName}", propertyInfo, parent);
        property.ConvertToAttribute(AttributeName, PropertyName);
        return property;
    }
}

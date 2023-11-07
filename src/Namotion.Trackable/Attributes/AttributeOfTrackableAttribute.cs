using Namotion.Trackable.Model;
using System;

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

    public override TrackedProperty CreateProperty(PropertyReflectionMetadata propertyReflectionMetadata, Tracker parent)
    {
        var property = new ReflectionTrackedProperty($"{PropertyName}.{AttributeName}", propertyReflectionMetadata, parent);
        property.ConvertToAttribute(AttributeName, PropertyName);
        return property;
    }
}

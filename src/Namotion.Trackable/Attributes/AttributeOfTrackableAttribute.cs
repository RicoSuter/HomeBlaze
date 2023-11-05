using Namotion.Trackable.Model;
using System;

namespace Namotion.Trackable.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class AttributeOfTrackableAttribute : TrackableAttribute, ITrackablePropertyInitializer
{
    public AttributeOfTrackableAttribute(string propertyName, string attributeName)
    {
        PropertyName = propertyName;
        AttributeName = attributeName;
    }

    public string PropertyName { get; }

    public string AttributeName { get; }

    public void InitializeProperty(TrackedProperty property, Tracker parent, object? parentCollectionKey, ITrackableContext context)
    {
        property.ToAttribute(AttributeName, PropertyName);
    }
}

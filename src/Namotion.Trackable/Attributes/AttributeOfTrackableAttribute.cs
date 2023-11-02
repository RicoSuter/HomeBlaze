using Namotion.Trackable.Model;
using System;
using System.Linq;

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

    public void InitializeProperty(ITrackableContext context, TrackedProperty property, Tracker parent, object? parentCollectionKey)
    {
        property.AttributeName = AttributeName;
        property.AttributedProperty = context
            .AllProperties
            .Single(v => v.Parent == property.Parent &&
                         v.PropertyName == PropertyName);
    }
}

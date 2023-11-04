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

    public void InitializeProperty(ITrackableContext context, TrackedProperty property, Tracker parent, object? parentCollectionKey)
    {
        property.AttributeName = AttributeName;
        property.AttributedProperty = property.Parent.TryGetProperty(PropertyName) ?? 
            throw new InvalidOperationException($"The attributed property {PropertyName} could not be found for attribute {AttributeName}.");
    }
}

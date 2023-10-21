using Namotion.Trackable.Model;
using System;
using System.Linq;

namespace Namotion.Trackable.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class AttributeOfTrackableAttribute : Attribute
{
    public AttributeOfTrackableAttribute(string propertyName, string attributeName)
    {
        PropertyName = propertyName;
        AttributeName = attributeName;
    }

    public string PropertyName { get; }

    public string AttributeName { get; }

    public TrackedProperty GetParentProperty(TrackedProperty property, ITrackableContext variablesContext)
    {
        return variablesContext.AllProperties
            .Single(v => v.Parent == property.Parent &&
                         v.PropertyName == PropertyName);
    }
}

using Namotion.Trackable.Model;
using System;
using System.Reflection;

namespace Namotion.Trackable.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TrackableAttribute : Attribute
{
    public virtual TrackedProperty CreateProperty(PropertyReflectionMetadata propertyReflectionMetadata, Tracker parent)
    {
        return new ReflectionTrackedProperty(propertyReflectionMetadata, parent);
    }
}

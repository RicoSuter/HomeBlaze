using Namotion.Trackable.Model;
using System;

namespace Namotion.Trackable.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TrackableAttribute : Attribute
{
    public virtual TrackedProperty CreateProperty(PropertyReflectionMetadata propertyReflectionMetadata, 
        ProxyTracker parent, IObserver<TrackedPropertyChange> observer)
    {
        return new ProxyTrackedProperty(propertyReflectionMetadata, parent, observer);
    }
}

using Namotion.Trackable.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Namotion.Trackable.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TrackableAttribute : Attribute, ITrackableAttribute
{
    public virtual TrackedProperty CreateTrackableProperty(PropertyInfo propertyInfo, string path, Tracker parent, int? parentCollectionIndex)
    {
        return new TrackedProperty(propertyInfo, path, parent);
    }

    public void ProcessProperty(TrackedProperty property, Tracker parent, int? parentCollectionIndex)
    {
        // auto create required properties
        if (property
                .GetCustomAttributes<Attribute>(true)
                .Any(a => a is RequiredAttribute ||
                          a.GetType().FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute") &&
            property.PropertyType.IsClass &&
            property.PropertyType.FullName?.StartsWith("System.") == false)
        {
            property.SetValue(parent.Context.CreateProxy(property.PropertyType));
        }
    }
}

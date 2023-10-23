using Namotion.Trackable.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Namotion.Trackable.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TrackableAttribute : Attribute
{
    [ThreadStatic]
    internal static TrackedProperty? CurrentTrackedProperty;

    public void CreateTrackableProperty(PropertyInfo propertyInfo, Tracker parent, int? parentCollectionIndex)
    {
        var propertyPath = GetPath(parent.Path, propertyInfo);

        var property = CreateTrackableProperty(propertyInfo, propertyPath, parent, parentCollectionIndex);

        CurrentTrackedProperty = property;
        try
        {
            parent.Properties.Add(property);

            foreach (var attribute in propertyInfo.GetCustomAttributes(true).OfType<ITrackableAttribute>())
            {
                attribute.ProcessProperty(property, parent, parentCollectionIndex);
            }

            // auto create required properties
            if (propertyInfo
                    .GetCustomAttributes(true)
                    .Any(a => a is RequiredAttribute ||
                              a.GetType().FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute") &&
                propertyInfo.PropertyType.IsClass &&
                propertyInfo.PropertyType.FullName?.StartsWith("System.") == false)
            {
                var child = parent.Context.CreateProxy(propertyInfo.PropertyType);

                parent.Context.CreateTracker(child, propertyPath, property, parentCollectionIndex: null);
                propertyInfo.SetValue(parent.Object, child);
            }
        }
        finally
        {
            CurrentTrackedProperty = null;
        }
    }

    protected virtual TrackedProperty CreateTrackableProperty(PropertyInfo property, string path, Tracker parent, int? parentCollectionIndex)
    {
        return new TrackedProperty(property, path, parent);
    }

    private string GetPath(string basePath, PropertyInfo propertyInfo)
    {
        // TODO: make shared method (AbsolutePath)
        return (!string.IsNullOrEmpty(basePath) ? basePath + "." : "") + propertyInfo.Name;
    }
}

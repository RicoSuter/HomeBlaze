using Namotion.Trackable.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Namotion.Trackable.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TrackableAttribute : Attribute
{
    public void CreateTrackableProperty(PropertyInfo propertyInfo, Tracker parent, object? parentCollectionKey)
    {
        var propertyPath = GetPath(parent.Path, propertyInfo);

        // TODO: Throw if not virtual property

        var property = CreateTrackableProperty(propertyInfo, propertyPath, parent, parentCollectionKey);
        parent.Properties.Add(property);

        foreach (var attribute in propertyInfo
            .GetCustomAttributes(true)
            .OfType<ITrackableAttribute>())
        {
            attribute.ProcessProperty(property, parent, parentCollectionKey);
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

            parent.Context.CreateTracker(child, propertyPath, property, parentCollectionKey: null);
            propertyInfo.SetValue(parent.Object, child);
        }

        // TODO: Also create arrays and dictionaries?
    }

    protected virtual TrackedProperty CreateTrackableProperty(PropertyInfo property, string path, Tracker parent, object? parentCollectionKey)
    {
        return new TrackedProperty(property, path, parent);
    }

    private string GetPath(string basePath, PropertyInfo propertyInfo)
    {
        // TODO: make shared method (AbsolutePath)
        return (!string.IsNullOrEmpty(basePath) ? basePath + "." : "") + propertyInfo.Name;
    }
}

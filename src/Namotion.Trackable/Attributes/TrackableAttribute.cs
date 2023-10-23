using Namotion.Trackable.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Namotion.Trackable.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TrackableAttribute : Attribute
{
    public IEnumerable<Tracker> CreateTrackersForProperty(PropertyInfo propertyInfo, Tracker parent, int? parentCollectionIndex)
    {
        var propertyPath = GetPath(parent.Path, propertyInfo);

        var property = CreateTrackableProperty(propertyInfo, propertyPath, parent, parentCollectionIndex);
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

            foreach (var childThing in parent.Context.CreateTrackers(child, propertyPath, property, parentCollectionIndex: null))
                yield return childThing;

            propertyInfo.SetValue(parent.Object, child);
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

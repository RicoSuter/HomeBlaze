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
    public IEnumerable<Tracker> CreateTrackersForProperty(PropertyInfo property, Tracker parent, int? parentCollectionIndex)
    {
        if (property.GetCustomAttribute<TrackableAttribute>(true) != null)
        {
            var propertyPath = GetPath(parent.Path, property);

            var trackableProperty = CreateTrackableProperty(property, propertyPath, parent, parentCollectionIndex);
            parent.Properties.Add(trackableProperty);

            if (property.GetCustomAttributes(true).Any(a => a is RequiredAttribute ||
                                                            a.GetType().FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute") &&
                property.PropertyType.IsClass &&
                property.PropertyType.FullName?.StartsWith("System.") == false)
            {
                var child = parent.Context.CreateProxy(property.PropertyType);

                foreach (var childThing in parent.Context.CreateTrackers(child, propertyPath, trackableProperty, parentCollectionIndex: null))
                    yield return childThing;

                property.SetValue(parent.Object, child);
            }
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

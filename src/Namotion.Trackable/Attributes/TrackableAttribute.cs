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
    public virtual IEnumerable<Model.Trackable> CreateTrackables(object proxy, PropertyInfo property, ITrackableContext context, Model.Trackable trackable)
    {
        var targetPath = GetTargetPath(trackable.Path, property);
        var sourcePath = GetSourcePath(trackable.SourcePath, property);

        if (property.GetCustomAttribute<TrackableAttribute>(true) != null &&
            property.GetCustomAttributes(true).Any(a => a is RequiredAttribute ||
                                                        a.GetType().FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute") &&
            property.PropertyType.IsClass &&
            property.PropertyType.FullName?.StartsWith("System.") == false)
        {
            var child = context.Create(property.PropertyType);

            foreach (var childThing in context.CreateThings(child, targetPath, sourcePath, trackable))
                yield return childThing;

            property.SetValue(proxy, child);
        }
        //else if (property.PropertyType.Type.IsArray)
        //{
        //    var attribute = property.GetContextAttribute<VariableSourceAttribute>();
        //    ArrayList items = (ArrayList)Activator.CreateInstance(typeof(ArrayList).MakeGenericType(property.PropertyType.EnumerableItemType), attribute.Length);
        //    for ( var i = 0; i < attribute.Length; i++)
        //    {
        //        var innerItem = (IGroup)ActivatorUtilities.CreateInstance(_serviceProvider, property.PropertyType.EnumerableItemType);
        //        innerItem.Parent = parent;
        //        var item = new ProxyGenerator()
        //            .CreateInterfaceProxyWithTarget(property.PropertyType.EnumerableItemType, innerItem, new ProxyGenerationOptions(), Interceptor);

        //        InitializeProperties(item, $"{parentSourcePath}.{property.Name}[{i}]", $"{parentTargetPath}.{property.Name}[{i}]");

        //        items.Add(item);
        //    }

        //    property.SetValue(parent, items.ToArray());
        //}
        else if (property.GetCustomAttribute<TrackableFromSourceAttribute>(true) != null)
        {
            trackable.Properties.Add(new TrackableProperty(context, trackable.Path, trackable.SourcePath, property, trackable));
        }
        else if (property.GetCustomAttribute<TrackableAttribute>(true) != null)
        {
            trackable.Properties.Add(new TrackableProperty(context, trackable.Path, null, property, trackable));
        }
        else if (property.GetCustomAttribute<AttributeOfTrackableAttribute>(true) != null)
        {
            trackable.Properties.Add(new TrackableProperty(context, trackable.Path, null, property, trackable));
        }
    }

    private string? GetSourcePath(string? basePath, PropertyInfo propertyInfo)
    {
        var attribute = propertyInfo.GetCustomAttribute<TrackableFromSourceAttribute>(true);
        if (attribute?.AbsolutePath != null)
        {
            return attribute?.AbsolutePath!;
        }
        else if (attribute?.RelativePath != null)
        {
            return (!string.IsNullOrEmpty(basePath) ? basePath + "." : "") + attribute?.RelativePath;
        }

        return (!string.IsNullOrEmpty(basePath) ? basePath + "." : "") + propertyInfo.Name;
    }

    private string GetTargetPath(string basePath, PropertyInfo propertyInfo)
    {
        return (!string.IsNullOrEmpty(basePath) ? basePath + "." : "") + propertyInfo.Name;
    }
}

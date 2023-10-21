using Namotion.Trackable.Attributes;
using Namotion.Trackable.Model;
using System;
using System.Reflection;

namespace Namotion.Trackable.Sourcing;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class TrackableFromSourceAttribute : TrackableAttribute
{
    public string? RelativePath { get; set; }

    public string? AbsolutePath { get; set; }

    protected override TrackedProperty CreateTrackableProperty(PropertyInfo propertyInfo, string path, Tracker parent, int? parentCollectionIndex)
    {
        if (propertyInfo.GetCustomAttribute<TrackableFromSourceAttribute>(true) != null)
        {
            var property = new TrackedProperty(propertyInfo, path, parent);

            var parentPath = parent.ParentProperty?.TryGetSourcePath() +
                (parentCollectionIndex != null ? $"[{parentCollectionIndex}]" : string.Empty);

            var sourcePath = GetSourcePath(parentPath, propertyInfo);
            property.SetSourcePath(sourcePath);

            return property;
        }
        else
        {
            return base.CreateTrackableProperty(propertyInfo, path, parent, parentCollectionIndex);
        }
    }

    private string GetSourcePath(string? basePath, PropertyInfo propertyInfo)
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
}

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

    public int Length { get; set; }

    protected override TrackedProperty CreateTrackableProperty(PropertyInfo property, string targetPath, Tracker parent, int? parentCollectionIndex, ITrackableContext context)
    {
        if (property.GetCustomAttribute<TrackableFromSourceAttribute>(true) != null)
        {
            var parentPath = parent.ParentProperty?.TryGetSourcePath() +
                (parentCollectionIndex != null ? $"[{parentCollectionIndex}]" : string.Empty);

            var sourcePath = GetSourcePath(parentPath, property);
            return new TrackedProperty(property, targetPath, parent, context)
            {
                Data = { { SourcingExtensions.SourcePathKey, sourcePath } }
            };
        }
        else
        {
            return base.CreateTrackableProperty(property, targetPath, parent, parentCollectionIndex, context);
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
}

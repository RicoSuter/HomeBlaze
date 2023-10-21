using Namotion.Trackable.Attributes;
using Namotion.Trackable.Model;
using System;
using System.Reflection;

namespace Namotion.Trackable.Sourcing;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class TrackableSourceAttribute : Attribute, ITrackableAttribute
{
    public string SourceName { get; }

    public string? RelativePath { get; set; }

    public string? AbsolutePath { get; set; }

    public TrackableSourceAttribute(string sourceName)
    {
        SourceName = sourceName;
    }

    public void OnTrackedPropertyCreated(TrackedProperty property, Tracker parent, int? parentCollectionIndex)
    {
        var parentPath = parent.ParentProperty?.TryGetSourcePath(SourceName) +
            (parentCollectionIndex != null ? $"[{parentCollectionIndex}]" : string.Empty);

        var sourcePath = GetSourcePath(parentPath, property);
        property.SetSourcePath(SourceName, sourcePath);
    }

    private string GetSourcePath(string? basePath, TrackedProperty property)
    {
        if (AbsolutePath != null)
        {
            return AbsolutePath!;
        }
        else if (RelativePath != null)
        {
            return (!string.IsNullOrEmpty(basePath) ? basePath + "." : "") + RelativePath;
        }

        return (!string.IsNullOrEmpty(basePath) ? basePath + "." : "") + property.PropertyName;
    }
}

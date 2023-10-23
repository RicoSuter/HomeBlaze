using Namotion.Trackable.Attributes;
using Namotion.Trackable.Model;
using System;

namespace Namotion.Trackable.Sourcing;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class TrackableSourceAttribute : Attribute, ITrackableAttribute
{
    public string SourceName { get; }

    public string? Path { get; }

    public string? AbsolutePath { get; set; }

    public TrackableSourceAttribute(string sourceName, string? path = null)
    {
        SourceName = sourceName;
        Path = path;
    }

    public void ProcessProperty(TrackedProperty property, Tracker parent, int? parentCollectionIndex)
    {
        var parentPath = parent.ParentProperty?.TryGetAttributeBasedSourcePathPrefix(SourceName) +
            (parentCollectionIndex != null ? $"[{parentCollectionIndex}]" : string.Empty);

        var sourcePath = GetSourcePath(parentPath, property);
        property.SetAttributeBasedSourcePath(SourceName, sourcePath);
    }

    private string GetSourcePath(string? basePath, TrackedProperty property)
    {
        if (AbsolutePath != null)
        {
            return AbsolutePath!;
        }
        else if (Path != null)
        {
            return (!string.IsNullOrEmpty(basePath) ? basePath + "." : "") + Path;
        }

        return (!string.IsNullOrEmpty(basePath) ? basePath + "." : "") + property.PropertyName;
    }
}

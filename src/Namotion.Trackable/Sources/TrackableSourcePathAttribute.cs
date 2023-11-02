using Namotion.Trackable.Attributes;
using Namotion.Trackable.Model;
using System;

namespace Namotion.Trackable.Sources;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class TrackableSourcePathAttribute : Attribute, ITrackablePropertyInitializer
{
    public string SourceName { get; }

    public string? Path { get; }

    public string? AbsolutePath { get; set; }

    public TrackableSourcePathAttribute(string sourceName, string? path = null)
    {
        SourceName = sourceName;
        Path = path;
    }

    public void InitializeProperty(ITrackableContext context, TrackedProperty property, Tracker parent, object? parentCollectionKey)
    {
        var parentPath = parent.ParentProperty?.TryGetAttributeBasedSourcePathPrefix(SourceName) +
            (parentCollectionKey != null ? $"[{parentCollectionKey}]" : string.Empty);

        var sourcePath = GetSourcePath(parentPath, property);
        property.SetAttributeBasedSourcePathPrefix(SourceName, sourcePath);
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
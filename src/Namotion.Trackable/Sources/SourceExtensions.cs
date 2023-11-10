using Namotion.Trackable.Model;
using System.Collections.Immutable;
using System.Linq;

namespace Namotion.Trackable.Sources;

public static class SourceExtensions
{
    private const string SourcePathKey = "SourcePath:";
    private const string SourcePathPrefixKey = "SourcePathPrefix:";

    private const string IsChangingFromSourceKey = "IsChangingFromSource";

    public static string? TryGetAttributeBasedSourcePath(this TrackedProperty property, string sourceName, ITrackableContext trackableContext)
    {
        lock (property.Data)
        {
            return property.Data.TryGetValue(SourcePathKey + sourceName, out var value) ? value as string : null;
        }
    }

    public static string? TryGetAttributeBasedSourcePathPrefix(this TrackedProperty property, string sourceName)
    {
        lock (property.Data)
        {
            return property.Data.TryGetValue(SourcePathPrefixKey + sourceName, out var value) ? value as string : null;
        }
    }

    public static void SetAttributeBasedSourcePathPrefix(this TrackedProperty property, string sourceName, string sourcePath)
    {
        lock (property.Data)
        {
            property.Data = property.Data.SetItem(SourcePathPrefixKey + sourceName, sourcePath);
        }
    }

    public static void SetAttributeBasedSourcePath(this TrackedProperty property, string sourceName, string sourcePath)
    {
        lock (property.Data)
        {
            property.Data = property.Data.SetItem(SourcePathKey + sourceName, sourcePath);
        }
    }

    public static void SetValueFromSource(this TrackedProperty property, ITrackableSource source, object? value)
    {
        lock (property.Data)
        {
            property.Data = property.Data.SetItem(IsChangingFromSourceKey, 
                property.Data.TryGetValue(IsChangingFromSourceKey, out var sources)
                ? ((ITrackableSource[])sources!).Append(source).ToArray()
                : (object)(new[] { source }));
        }

        try
        {
            var currentValue = property.Value;
            if (!Equals(currentValue, value))
            {
                property.                Value=value;
            }
        }
        finally
        {
            lock (property.Data)
            {
                property.Data = property.Data.SetItem(IsChangingFromSourceKey,
                    ((ITrackableSource[])property.Data[IsChangingFromSourceKey]!)
                    .Except(new[] { source })
                    .ToArray());
            }
        }
    }

    public static bool IsChangingFromSource(this TrackedPropertyChange change, ITrackableSource source)
    {
        return change.PropertyDataSnapshot.TryGetValue(IsChangingFromSourceKey, out var isChangingFromSource) &&
            isChangingFromSource is ITrackableSource[] sources &&
            sources.Contains(source);
    }
}

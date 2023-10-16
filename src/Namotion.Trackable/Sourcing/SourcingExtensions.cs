using Namotion.Trackable.Model;

namespace Namotion.Trackable.Sourcing;

public static class SourcingExtensions
{
    internal const string SourcePathKey = "SourcePath";

    public static string? TryGetSourcePath(this TrackableProperty property)
    {
        return property.ExtensionData.TryGetValue(SourcePathKey, out var value) ? value as string : null;
    }
}

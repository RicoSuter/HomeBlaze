using Namotion.Trackable.Model;

namespace Namotion.Trackable.Sourcing;

public static class SourcingExtensions
{
    internal const string SourcePathKey = "SourcePath";

    public static string? TryGetSourcePath(this Model.Trackable trackable)
    {
        return trackable.ExtensionData[SourcePathKey] as string;
    }

    public static string? TryGetSourcePath(this TrackableProperty property)
    {
        return property.ExtensionData[SourcePathKey] as string;
    }
}

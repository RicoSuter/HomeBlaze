using Namotion.Trackable.Model;

namespace Namotion.Trackable.Sources;

public interface ISourcePathProvider
{
    string? TryGetSourceProperty(TrackedProperty property);

    string? TryGetSourcePath(TrackedProperty property);
}

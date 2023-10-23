using Namotion.Trackable.Model;

namespace Namotion.Trackable.Sources;

public interface ISourcePathProvider
{
    string? TryGetSourcePath(TrackedProperty property);
}

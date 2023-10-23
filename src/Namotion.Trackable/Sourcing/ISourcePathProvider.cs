using Namotion.Trackable.Model;

namespace Namotion.Trackable.Sourcing;

public interface ISourcePathProvider
{
    string? TryGetSourcePath(TrackedProperty property);
}

using Namotion.Trackable.Model;

namespace Namotion.Trackable.Sources;

public class AttributeBasedSourcePathProvider : ISourcePathProvider
{
    private string _sourceName;
    private readonly ITrackableContext _trackableContext;

    public AttributeBasedSourcePathProvider(string sourceName, ITrackableContext trackableContext)
    {
        _sourceName = sourceName;
        _trackableContext = trackableContext;
    }

    public string? TryGetSourcePath(TrackedProperty property)
    {
        return property.TryGetAttributeBasedSourcePath(_sourceName, _trackableContext);
    }
}

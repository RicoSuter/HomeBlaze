using Namotion.Trackable.Model;

namespace Namotion.Trackable.Sources;

public class AttributeBasedSourcePathProvider : ISourcePathProvider
{
    private string _sourceName;
    private readonly ITrackableContext _trackableContext;
    private readonly string? _pathPrefix;

    public AttributeBasedSourcePathProvider(string sourceName, ITrackableContext trackableContext, string? pathPrefix = null)
    {
        _sourceName = sourceName;
        _trackableContext = trackableContext;
        _pathPrefix = pathPrefix ?? string.Empty;
    }

    public string? TryGetSourceProperty(TrackedProperty property)
    {
        var propertyName = property.TryGetAttributeBasedSourceProperty(_sourceName);
        return propertyName is not null ? propertyName : null;
    }

    public string? TryGetSourcePath(TrackedProperty property)
    {
        var path = property.TryGetAttributeBasedSourcePath(_sourceName, _trackableContext);
        return path is not null ? _pathPrefix + path : null;
    }
}

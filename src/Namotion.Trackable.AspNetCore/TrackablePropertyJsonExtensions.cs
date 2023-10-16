using System.Linq;
using System.Text.Json;

using Namotion.Trackable.Model;

namespace Namotion.Trackable.AspNetCore;

public static class TrackablePropertyJsonExtensions
{
    public static string GetJsonPath(this TrackableProperty trackableProperty)
    {
        if (trackableProperty.IsAttribute)
        {
            var variable = trackableProperty.AttributeMetadata
                .GetParent(trackableProperty, trackableProperty.Context);

            return string.Join('.', variable.Path.Split('.').Select(s => JsonNamingPolicy.CamelCase.ConvertName(s)))
                + "@" + JsonNamingPolicy.CamelCase.ConvertName(trackableProperty.AttributeMetadata!.AttributeName);
        }
        else
        {
            return string.Join('.', trackableProperty.Path.Split('.').Select(s => JsonNamingPolicy.CamelCase.ConvertName(s)));
        }
    }
}

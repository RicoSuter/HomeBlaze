using System.Linq;
using System.Text.Json;

using Namotion.Trackable.Model;

namespace Namotion.Trackable.AspNetCore;

public static class TrackablePropertyJsonExtensions
{
    public static string GetJsonPath(this TrackedProperty property)
    {
        if (property.IsAttribute)
        {
            var variable = property.AttributeMetadata
                .GetParentProperty(property, property.Context);

            return string.Join('.', variable.Path.Split('.').Select(s => JsonNamingPolicy.CamelCase.ConvertName(s)))
                + "@" + JsonNamingPolicy.CamelCase.ConvertName(property.AttributeMetadata!.AttributeName);
        }
        else
        {
            return string.Join('.', property.Path.Split('.').Select(s => JsonNamingPolicy.CamelCase.ConvertName(s)));
        }
    }
}

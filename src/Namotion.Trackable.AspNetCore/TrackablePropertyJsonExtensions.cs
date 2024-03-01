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
            return
                string
                    .Join('.', property.AttributedProperty.Path.Split('.')
                    .Select(s => JsonNamingPolicy.CamelCase.ConvertName(s)))
                + "@" + JsonNamingPolicy.CamelCase.ConvertName(property.AttributeName);
        }
        else
        {
            return string
                .Join('.', property.Path.Split('.')
                .Select(s => JsonNamingPolicy.CamelCase.ConvertName(s)));
        }
    }
}

using System.Text.Json.Serialization;

namespace Namotion.Trackable.Utilities;

public interface ITrackableWithParent
{
    [JsonIgnore]
    object? Parent { get; set; }
}

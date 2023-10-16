using System.Text.Json.Serialization;

namespace Namotion.Trackable.Utilities;

public class TrackableWithParent<TParent> : ITrackableWithParent
{
    [JsonIgnore]
    public TParent? Parent { get; set; }

    [JsonIgnore]
    object? ITrackableWithParent.Parent
    {
        get { return Parent; }
        set { Parent = (TParent?)value; }
    }
}

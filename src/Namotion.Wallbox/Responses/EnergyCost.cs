using System.Text.Json.Serialization;

namespace Namotion.Wallbox.Responses
{
    public class EnergyCost
    {
        [JsonPropertyName("value")]
        public float Value { get; set; }

        [JsonPropertyName("inheritedGroupId")]
        public int? InheritedGroupId { get; set; }
    }




}

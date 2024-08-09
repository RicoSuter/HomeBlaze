using System.Text.Json.Serialization;

namespace Namotion.Wallbox.Responses.GetChargerStatus
{
    public class EnergyCost
    {
        [JsonPropertyName("value")]
        public decimal Value { get; set; }

        [JsonPropertyName("inheritedGroupId")]
        public int? InheritedGroupId { get; set; }
    }
}

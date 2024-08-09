using System.Text.Json.Serialization;

namespace Namotion.Wallbox.Responses.GetChargerStatus
{
    public class Plan
    {
        [JsonPropertyName("plan_name")]
        public string? PlanName { get; set; }

        [JsonPropertyName("features")]
        public string[]? Features { get; set; }
    }
}

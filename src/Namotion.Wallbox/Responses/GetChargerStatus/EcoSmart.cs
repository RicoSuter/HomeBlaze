using System.Text.Json.Serialization;

namespace Namotion.Wallbox.Responses.GetChargerStatus
{
    public class EcoSmart
    {
        [JsonPropertyName("enabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("mode")]
        public int Mode { get; set; }

        [JsonPropertyName("percentage")]
        public int Percentage { get; set; }
    }
}

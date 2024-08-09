using System.Text.Json.Serialization;

namespace Namotion.Wallbox.Responses.GetChargerStatus
{
    public class Software
    {
        [JsonPropertyName("updateAvailable")]
        public bool UpdateAvailable { get; set; }

        [JsonPropertyName("currentVersion")]
        public string? CurrentVersion { get; set; }

        [JsonPropertyName("latestVersion")]
        public string? LatestVersion { get; set; }

        [JsonPropertyName("fileName")]
        public string? FileName { get; set; }
    }
}

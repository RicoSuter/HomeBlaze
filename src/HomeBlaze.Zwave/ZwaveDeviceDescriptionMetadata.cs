using System.Text.Json.Serialization;

namespace HomeBlaze.Zwave
{
    public class ZwaveDeviceDescriptionMetadata
    {
        [JsonPropertyName("reset")]
        public string? Reset { get; set; }

        [JsonPropertyName("inclusion")]
        public string? Inclusion { get; set; }

        [JsonPropertyName("exclusion")]
        public string? Exclusion { get; set; }

        [JsonPropertyName("manual")]
        public string? Manual { get; set; }
    }
}

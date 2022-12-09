using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class SoftwareUpdate
    {
        [JsonPropertyName("download_perc"), State]
        public long Download_perc { get; set; }

        [JsonPropertyName("expected_duration_sec"), State]
        public long Expected_duration_sec { get; set; }

        [JsonPropertyName("install_perc"), State]
        public long Install_perc { get; set; }

        [JsonPropertyName("status"), State]
        public string? Status { get; set; }

        [JsonPropertyName("version"), State]
        public string? Version { get; set; }
    }
}
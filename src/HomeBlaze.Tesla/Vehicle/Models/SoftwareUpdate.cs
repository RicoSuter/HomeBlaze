using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class SoftwareUpdate
    {
        [JsonPropertyName("download_perc"), State]
        public long DownloadPercent { get; set; }

        [JsonPropertyName("expected_duration_sec"), State]
        public long ExpectedDurationSeconds{ get; set; }

        [JsonPropertyName("install_perc"), State]
        public long InstallPercent { get; set; }

        [JsonPropertyName("status"), State]
        public string? Status { get; set; }

        [JsonPropertyName("version"), State]
        public string? Version { get; set; }
    }
}
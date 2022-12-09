using System.Text.Json.Serialization;

namespace HomeBlaze.Nuki.Model
{
    public partial class Versions
    {
        [JsonPropertyName("firmwareVersion")]
        public string? FirmwareVersion { get; set; }

        [JsonPropertyName("wifiFirmwareVersion")]
        public string? WifiFirmwareVersion { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace HomeBlaze.Nuki.Model
{
    public partial class NukiDevice
    {
        [JsonPropertyName("deviceType")]
        public int DeviceType { get; set; }

        [JsonPropertyName("nukiId")]
        public int NukiId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("firmwareVersion")]
        public string? FirmwareVersion { get; set; }

        [JsonPropertyName("lastKnownState")]
        public LastKnownState? LastKnownState { get; set; }
    }
}

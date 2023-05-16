using System.Text.Json.Serialization;

namespace HomeBlaze.Nuki.Model
{
    public partial class ScanResult
    {
        [JsonPropertyName("deviceType")]
        public int DeviceType { get; set; }

        [JsonPropertyName("nukiId")]
        public int NukiId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("rssi")]
        public int Rssi { get; set; }

        [JsonPropertyName("paired")]
        public bool Paired { get; set; }
    }
}

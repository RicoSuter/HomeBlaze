using System.Text.Json.Serialization;

namespace HomeBlaze.Zwave
{
    public class ZwaveDeviceDescription
    {
        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("manufacturer")]
        public string? Manufacturer { get; set; }

        [JsonPropertyName("manufacturerId")]
        public string? ManufacturerId { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("devices")]
        public ZwaveDeviceDescriptionItem[]? Devices { get; set; }

        [JsonPropertyName("metadata")]
        public ZwaveDeviceDescriptionMetadata? Metadata { get; set; }
    }
}

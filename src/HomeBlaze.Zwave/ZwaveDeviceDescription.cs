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

    public class ZwaveDeviceDescriptionItem
    {
        [JsonPropertyName("productType")]
        public string? ProductType { get; set; }

        [JsonPropertyName("productId")]
        public string? ProductId { get; set; }
    }

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

using System.Text.Json.Serialization;

namespace HomeBlaze.Zwave
{
    public class ZwaveDeviceDescriptionItem
    {
        [JsonPropertyName("productType")]
        public string? ProductType { get; set; }

        [JsonPropertyName("productId")]
        public string? ProductId { get; set; }
    }
}

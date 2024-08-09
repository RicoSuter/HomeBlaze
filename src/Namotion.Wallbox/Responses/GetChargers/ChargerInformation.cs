using System.Text.Json.Serialization;

namespace Namotion.Wallbox.Responses.GetChargers
{
    public class ChargerInformation
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("connection_status")]
        public string? ConnectionStatus { get; set; }

        [JsonPropertyName("location_name")]
        public string? LocationName { get; set; }

        [JsonPropertyName("serial_number")]
        public string? SerialNumber { get; set; }
    }
}

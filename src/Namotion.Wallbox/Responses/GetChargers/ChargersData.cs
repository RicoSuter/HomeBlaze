using System.Text.Json.Serialization;

namespace Namotion.Wallbox.Responses.GetChargers
{
    public class ChargersData
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("attributes")]
        public ChargerAttributes? Attributes { get; set; }
    }
}

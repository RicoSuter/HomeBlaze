using System.Text.Json.Serialization;

namespace Namotion.Wallbox.Responses.GetChargerStatus
{
    public class Currency
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }
    }
}

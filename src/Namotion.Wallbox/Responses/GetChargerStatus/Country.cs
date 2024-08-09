using System.Text.Json.Serialization;

namespace Namotion.Wallbox.Responses.GetChargerStatus
{
    public class Country
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("iso2")]
        public string? Iso2 { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("phone_code")]
        public string? PhoneCode { get; set; }
    }
}

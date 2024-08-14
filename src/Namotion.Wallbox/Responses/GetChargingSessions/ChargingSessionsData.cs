using System.Text.Json.Serialization;

namespace Namotion.Wallbox.Responses.GetChargingSessions
{
    public class ChargingSessionsData
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("attributes")]
        public required ChargingSessionAttributes Attributes { get; set; }
    }
}
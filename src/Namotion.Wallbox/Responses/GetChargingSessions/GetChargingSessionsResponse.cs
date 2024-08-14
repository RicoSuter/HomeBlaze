using System.Text.Json.Serialization;

namespace Namotion.Wallbox.Responses.GetChargingSessions
{
    public class GetChargingSessionsResponse
    {
        [JsonPropertyName("data")]
        public ChargingSessionsData[]? Data { get; set; }
    }
}

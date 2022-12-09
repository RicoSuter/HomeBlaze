using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.WallConnector
{
    public class TeslaWallConnectorLifetime
    {
        [JsonPropertyName("energy_wh")]
        public double TotalConsumedPower { get; set; }

        [JsonPropertyName("uptime_s")]
        public long UptimeSeconds { get; set; }
    }
}
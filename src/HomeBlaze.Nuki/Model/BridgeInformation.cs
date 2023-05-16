using System;
using System.Text.Json.Serialization;

namespace HomeBlaze.Nuki.Model
{
    public partial class BridgeInformation
    {
        [JsonPropertyName("bridgeType")]
        public int BridgeType { get; set; }

        [JsonPropertyName("ids")]
        public Ids? Ids { get; set; }

        [JsonPropertyName("versions")]
        public Versions? Versions { get; set; }

        [JsonPropertyName("uptime")]
        public int Uptime { get; set; }

        [JsonPropertyName("currentTime")]
        public DateTimeOffset CurrentTime { get; set; }

        [JsonPropertyName("wlanConnected")]
        public bool WlanConnected { get; set; }

        [JsonPropertyName("serverConnected")]
        public bool ServerConnected { get; set; }

        [JsonPropertyName("scanResults")]
        public ScanResult[]? ScanResults { get; set; }
    }
}

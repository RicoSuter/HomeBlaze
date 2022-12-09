using System;
using System.Text.Json.Serialization;

namespace HomeBlaze.Nuki.Model
{
    public partial class BridgeModel
    {
        [JsonPropertyName("bridgeId")]
        public int BridgeId { get; set; }

        [JsonPropertyName("ip")]
        public string? Ip { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonIgnore]
        public string Host => Ip + ":" + Port;

        [JsonPropertyName("dateUpdated")]
        public DateTimeOffset DateUpdated { get; set; }
    }
}

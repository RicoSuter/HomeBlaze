using System.Text.Json.Serialization;

namespace HomeBlaze.Nuki.Model
{
    public partial class DiscoverResult
    {
        [JsonPropertyName("bridges")]
        public BridgeModel[]? Bridges { get; set; }

        [JsonPropertyName("errorCode")]
        public int ErrorCode { get; set; }
    }
}

using System.Text.Json.Serialization;

namespace HomeBlaze.Nuki.Model
{
    public partial class Ids
    {
        [JsonPropertyName("hardwareId")]
        public int HardwareId { get; set; }

        [JsonPropertyName("serverId")]
        public int ServerId { get; set; }
    }
}

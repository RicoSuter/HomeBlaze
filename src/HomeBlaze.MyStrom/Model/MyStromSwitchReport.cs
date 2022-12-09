using System.Text.Json.Serialization;

namespace HomeBlaze.MyStrom.Model
{
    internal class MyStromSwitchReport
    {
        [JsonPropertyName("power")]
        public decimal Power { get; set; }

        [JsonPropertyName("Ws")]
        public decimal Ws { get; set; }

        [JsonPropertyName("relay")]
        public bool Relay { get; set; }

        [JsonPropertyName("temperature")]
        public decimal Temperature { get; set; }
    }
}

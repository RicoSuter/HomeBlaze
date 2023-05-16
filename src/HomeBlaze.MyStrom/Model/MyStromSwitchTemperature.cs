using System.Text.Json.Serialization;

namespace HomeBlaze.MyStrom.Model
{
    internal class MyStromSwitchTemperature
    {
        [JsonPropertyName("measured")]
        public decimal Measured { get; set; }

        [JsonPropertyName("compensation")]
        public decimal Compensation { get; set; }

        [JsonPropertyName("compensated")]
        public decimal Compensated { get; set; }

        [JsonPropertyName("offset")]
        public decimal? Offset { get; set; }
    }
}

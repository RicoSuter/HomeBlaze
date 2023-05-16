using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class SpeedLimitMode
    {
        [JsonPropertyName("active"), State]
        public bool Active { get; set; }

        [JsonPropertyName("current_limit_mph"), State]
        public double CurrentLimitMph { get; set; }

        [JsonPropertyName("max_limit_mph"), State]
        public long MaximumLimitMph { get; set; }

        [JsonPropertyName("min_limit_mph"), State]
        public double MinimumLimitMph { get; set; }

        [JsonPropertyName("pin_code_set"), State]
        public bool IsPinCodeSet { get; set; }
    }
}
using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class SpeedLimitMode
    {
        [JsonPropertyName("active"), State]
        public bool Active { get; set; }

        [JsonPropertyName("current_limit_mph"), State]
        public double Current_limit_mph { get; set; }

        [JsonPropertyName("max_limit_mph"), State]
        public long Max_limit_mph { get; set; }

        [JsonPropertyName("min_limit_mph"), State]
        public double Min_limit_mph { get; set; }

        [JsonPropertyName("pin_code_set"), State]
        public bool Pin_code_set { get; set; }
    }
}
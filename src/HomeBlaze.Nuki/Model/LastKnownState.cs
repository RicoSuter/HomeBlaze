using System;
using System.Text.Json.Serialization;

namespace HomeBlaze.Nuki.Model
{
    public partial class LastKnownState
    {
        [JsonPropertyName("mode")]
        public int Mode { get; set; }

        [JsonPropertyName("state")]
        public int State { get; set; }

        [JsonPropertyName("stateName")]
        public string? StateName { get; set; }

        [JsonPropertyName("batteryCritical")]
        public bool BatteryCritical { get; set; }

        [JsonPropertyName("batteryCharging")]
        public bool BatteryCharging { get; set; }

        [JsonPropertyName("batteryChargeState")]
        public int BatteryChargeState { get; set; }

        [JsonPropertyName("doorsensorState")]
        public int DoorsensorState { get; set; }

        [JsonPropertyName("doorsensorStateName")]
        public string? DoorsensorStateName { get; set; }

        [JsonPropertyName("timestamp")]
        public DateTimeOffset? Timestamp { get; set; }
    }
}

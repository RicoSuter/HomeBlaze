using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class GuiSettings
    {
        [JsonPropertyName("gui_24_hour_time"), State]
        public bool UseGui24HourTime { get; set; }

        [JsonPropertyName("gui_charge_rate_units"), State]
        public string? GuiChargeRateUnits { get; set; }

        [JsonPropertyName("gui_distance_units"), State]
        public string? GuiDistanceUnits { get; set; }

        [JsonPropertyName("gui_range_display"), State]
        public string? GuiRangeDisplay { get; set; }

        [JsonPropertyName("gui_temperature_units"), State]
        public string? GuiTemperatureUnits { get; set; }

        [JsonPropertyName("gui_tirepressure_units"), State]
        public string? GuiTirePressureUnits { get; set; }

        [JsonPropertyName("show_range_units"), State]
        public bool ShowRangeUnits { get; set; }

        [JsonPropertyName("timestamp"), State]
        public long Timestamp { get; set; }
    }
}
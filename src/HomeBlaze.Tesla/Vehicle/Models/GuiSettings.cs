using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class GuiSettings
    {
        [JsonPropertyName("gui_24_hour_time"), State]
        public bool Gui_24_hour_time { get; set; }

        [JsonPropertyName("gui_charge_rate_units"), State]
        public string? Gui_charge_rate_units { get; set; }

        [JsonPropertyName("gui_distance_units"), State]
        public string? Gui_distance_units { get; set; }

        [JsonPropertyName("gui_range_display"), State]
        public string? Gui_range_display { get; set; }

        [JsonPropertyName("gui_temperature_units"), State]
        public string? Gui_temperature_units { get; set; }

        [JsonPropertyName("gui_tirepressure_units"), State]
        public string? Gui_tirepressure_units { get; set; }

        [JsonPropertyName("show_range_units"), State]
        public bool Show_range_units { get; set; }

        [JsonPropertyName("timestamp"), State]
        public long Timestamp { get; set; }
    }
}
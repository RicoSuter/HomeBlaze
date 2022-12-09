using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class DriveState
    {
        [JsonPropertyName("active_route_latitude"), State]
        public double Active_route_latitude { get; set; }

        [JsonPropertyName("active_route_longitude"), State]
        public double Active_route_longitude { get; set; }

        [JsonPropertyName("active_route_traffic_minutes_delay"), State]
        public double Active_route_traffic_minutes_delay { get; set; }

        [JsonPropertyName("gps_as_of"), State]
        public long Gps_as_of { get; set; }

        [JsonPropertyName("heading"), State]
        public long Heading { get; set; }

        [JsonPropertyName("latitude"), State]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude"), State]
        public double Longitude { get; set; }

        [JsonPropertyName("native_latitude"), State]
        public double Native_latitude { get; set; }

        [JsonPropertyName("native_location_supported"), State]
        public long Native_location_supported { get; set; }

        [JsonPropertyName("native_longitude"), State]
        public double Native_longitude { get; set; }

        [JsonPropertyName("native_type"), State]
        public string? Native_type { get; set; }

        [JsonPropertyName("power"), State]
        public long Power { get; set; }

        [JsonPropertyName("shift_state"), State]
        public object? Shift_state { get; set; }

        [JsonPropertyName("speed"), State]
        public object? Speed { get; set; }

        [JsonPropertyName("timestamp"), State]
        public long Timestamp { get; set; }
    }
}
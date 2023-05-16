using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class DriveState : IThing
    {
        public string Id => Parent!.Id + "/drive";

        public string? Title => "Drive";

        [ParentThing]
        public TeslaVehicle? Parent { get; private set; }


        [State]
        public GeoCoordinate ActiveRouteLocation => new()
        {
            Latitude = ActiveRouteLatitude,
            Longitude = ActiveRouteLongitude
        };

        [JsonPropertyName("active_route_latitude")]
        public double ActiveRouteLatitude { get; set; }

        [JsonPropertyName("active_route_longitude")]
        public double ActiveRouteLongitude { get; set; }

        [JsonPropertyName("active_route_traffic_minutes_delay"), State]
        public double ActiveRouteTrafficMinutesDelay { get; set; }

        [JsonPropertyName("gps_as_of"), State]
        public long GpsAsOf { get; set; }

        [JsonPropertyName("heading"), State]
        public long Heading { get; set; }

        [State]
        public GeoCoordinate Location => new()
        {
            Latitude = Latitude,
            Longitude = Longitude
        };

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [State]
        public GeoCoordinate NativeLocation => new()
        {
            Latitude = NativeLatitude,
            Longitude = NativeLongitude
        };

        [JsonPropertyName("native_latitude")]
        public double NativeLatitude { get; set; }

        [JsonPropertyName("native_longitude")]
        public double NativeLongitude { get; set; }

        [JsonPropertyName("native_location_supported"), State]
        public long NativeLocationSupported { get; set; }

        [JsonPropertyName("native_type"), State]
        public string? NativeType { get; set; }

        [JsonPropertyName("power"), State]
        public long Power { get; set; }

        [JsonPropertyName("shift_state"), State]
        public object? ShiftState { get; set; }

        [JsonPropertyName("speed"), State]
        public object? Speed { get; set; }

        [JsonPropertyName("timestamp"), State]
        public long Timestamp { get; set; }
    }
}
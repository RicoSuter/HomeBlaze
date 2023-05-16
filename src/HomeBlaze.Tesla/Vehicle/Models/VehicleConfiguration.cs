using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class VehicleConfiguration
    {
        [JsonPropertyName("aux_park_lamps"), State]
        public string? Aux_park_lamps { get; set; }

        [JsonPropertyName("badge_version"), State]
        public long Badge_version { get; set; }

        [JsonPropertyName("can_accept_navigation_requests"), State]
        public bool Can_accept_navigation_requests { get; set; }

        [JsonPropertyName("can_actuate_trunks"), State]
        public bool Can_actuate_trunks { get; set; }

        [JsonPropertyName("car_special_type"), State]
        public string? Car_special_type { get; set; }

        [JsonPropertyName("car_type"), State]
        public string? Car_type { get; set; }

        [JsonPropertyName("charge_port_type"), State]
        public string? Charge_port_type { get; set; }

        [JsonPropertyName("cop_user_set_temp_supported"), State]
        public bool Cop_user_set_temp_supported { get; set; }

        [JsonPropertyName("dashcam_clip_save_supported"), State]
        public bool Dashcam_clip_save_supported { get; set; }

        [JsonPropertyName("default_charge_to_max"), State]
        public bool Default_charge_to_max { get; set; }

        [JsonPropertyName("driver_assist"), State]
        public string? Driver_assist { get; set; }

        [JsonPropertyName("ece_restrictions"), State]
        public bool Ece_restrictions { get; set; }

        [JsonPropertyName("efficiency_package"), State]
        public string? Efficiency_package { get; set; }

        [JsonPropertyName("eu_vehicle"), State]
        public bool Eu_vehicle { get; set; }

        [JsonPropertyName("exterior_color"), State]
        public string? Exterior_color { get; set; }

        [JsonPropertyName("exterior_trim"), State]
        public string? Exterior_trim { get; set; }

        [JsonPropertyName("exterior_trim_override"), State]
        public string? Exterior_trim_override { get; set; }

        [JsonPropertyName("has_air_suspension"), State]
        public bool Has_air_suspension { get; set; }

        [JsonPropertyName("has_ludicrous_mode"), State]
        public bool Has_ludicrous_mode { get; set; }

        [JsonPropertyName("has_seat_cooling"), State]
        public bool Has_seat_cooling { get; set; }

        [JsonPropertyName("headlamp_type"), State]
        public string? Headlamp_type { get; set; }

        [JsonPropertyName("interior_trim_type"), State]
        public string? Interior_trim_type { get; set; }

        [JsonPropertyName("key_version"), State]
        public long Key_version { get; set; }

        [JsonPropertyName("motorized_charge_port"), State]
        public bool Motorized_charge_port { get; set; }

        [JsonPropertyName("paint_color_override"), State]
        public string? Paint_color_override { get; set; }

        [JsonPropertyName("performance_package"), State]
        public string? Performance_package { get; set; }

        [JsonPropertyName("plg"), State]
        public bool Plg { get; set; }

        [JsonPropertyName("pws"), State]
        public bool Pws { get; set; }

        [JsonPropertyName("rear_drive_unit"), State]
        public string? Rear_drive_unit { get; set; }

        [JsonPropertyName("rear_seat_heaters"), State]
        public long Rear_seat_heaters { get; set; }

        [JsonPropertyName("rear_seat_type"), State]
        public long Rear_seat_type { get; set; }

        [JsonPropertyName("rhd"), State]
        public bool Rhd { get; set; }

        [JsonPropertyName("roof_color"), State]
        public string? Roof_color { get; set; }

        [JsonPropertyName("seat_type"), State]
        public object? Seat_type { get; set; }

        [JsonPropertyName("spoiler_type"), State]
        public string? Spoiler_type { get; set; }

        [JsonPropertyName("sun_roof_installed"), State]
        public object? Sun_roof_installed { get; set; }

        [JsonPropertyName("supports_qr_pairing"), State]
        public bool Supports_qr_pairing { get; set; }

        [JsonPropertyName("third_row_seats"), State]
        public string? Third_row_seats { get; set; }

        [JsonPropertyName("timestamp"), State]
        public long Timestamp { get; set; }

        [JsonPropertyName("trim_badging"), State]
        public string? Trim_badging { get; set; }

        [JsonPropertyName("use_range_badging"), State]
        public bool Use_range_badging { get; set; }

        [JsonPropertyName("utc_offset"), State]
        public long Utc_offset { get; set; }

        [JsonPropertyName("webcam_supported"), State]
        public bool Webcam_supported { get; set; }

        [JsonPropertyName("wheel_type"), State]
        public string? Wheel_type { get; set; }
    }
}
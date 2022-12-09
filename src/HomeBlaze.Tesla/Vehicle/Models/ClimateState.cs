using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class ClimateState
    {
        [JsonPropertyName("allow_cabin_overheat_protection"), State]
        public bool Allow_cabin_overheat_protection { get; set; }

        [JsonPropertyName("auto_seat_climate_left"), State]
        public bool Auto_seat_climate_left { get; set; }

        [JsonPropertyName("auto_seat_climate_right"), State]
        public bool Auto_seat_climate_right { get; set; }

        [JsonPropertyName("battery_heater"), State]
        public bool Battery_heater { get; set; }

        [JsonPropertyName("battery_heater_no_power"), State]
        public object? Battery_heater_no_power { get; set; }

        [JsonPropertyName("cabin_overheat_protection"), State]
        public string? Cabin_overheat_protection { get; set; }

        [JsonPropertyName("cabin_overheat_protection_actively_cooling"), State]
        public bool Cabin_overheat_protection_actively_cooling { get; set; }

        [JsonPropertyName("climate_keeper_mode"), State]
        public string? Climate_keeper_mode { get; set; }

        [JsonPropertyName("cop_activation_temperature"), State]
        public string? Cop_activation_temperature { get; set; }

        [JsonPropertyName("defrost_mode"), State]
        public long Defrost_mode { get; set; }

        [JsonPropertyName("driver_temp_setting"), State]
        public double Driver_temp_setting { get; set; }

        [JsonPropertyName("fan_status"), State]
        public long Fan_status { get; set; }

        [JsonPropertyName("hvac_auto_request"), State]
        public string? Hvac_auto_request { get; set; }

        [JsonPropertyName("inside_temp"), State]
        public double Inside_temp { get; set; }

        [JsonPropertyName("is_auto_conditioning_on"), State]
        public bool Is_auto_conditioning_on { get; set; }

        [JsonPropertyName("is_climate_on"), State]
        public bool Is_climate_on { get; set; }

        [JsonPropertyName("is_front_defroster_on"), State]
        public bool Is_front_defroster_on { get; set; }

        [JsonPropertyName("is_preconditioning"), State]
        public bool Is_preconditioning { get; set; }

        [JsonPropertyName("is_rear_defroster_on"), State]
        public bool Is_rear_defroster_on { get; set; }

        [JsonPropertyName("left_temp_direction"), State]
        public long Left_temp_direction { get; set; }

        [JsonPropertyName("max_avail_temp"), State]
        public double Max_avail_temp { get; set; }

        [JsonPropertyName("min_avail_temp"), State]
        public double Min_avail_temp { get; set; }

        [JsonPropertyName("outside_temp"), State]
        public double Outside_temp { get; set; }

        [JsonPropertyName("passenger_temp_setting"), State]
        public double Passenger_temp_setting { get; set; }

        [JsonPropertyName("remote_heater_control_enabled"), State]
        public bool Remote_heater_control_enabled { get; set; }

        [JsonPropertyName("right_temp_direction"), State]
        public long Right_temp_direction { get; set; }

        [JsonPropertyName("seat_heater_left"), State]
        public long Seat_heater_left { get; set; }

        [JsonPropertyName("seat_heater_rear_center"), State]
        public long Seat_heater_rear_center { get; set; }

        [JsonPropertyName("seat_heater_rear_left"), State]
        public long Seat_heater_rear_left { get; set; }

        [JsonPropertyName("seat_heater_rear_right"), State]
        public long Seat_heater_rear_right { get; set; }

        [JsonPropertyName("seat_heater_right"), State]
        public long Seat_heater_right { get; set; }

        [JsonPropertyName("side_mirror_heaters"), State]
        public bool Side_mirror_heaters { get; set; }

        [JsonPropertyName("steering_wheel_heater"), State]
        public bool Steering_wheel_heater { get; set; }

        [JsonPropertyName("supports_fan_only_cabin_overheat_protection"), State]
        public bool Supports_fan_only_cabin_overheat_protection { get; set; }

        [JsonPropertyName("timestamp"), State]
        public long Timestamp { get; set; }

        [JsonPropertyName("wiper_blade_heater"), State]
        public bool Wiper_blade_heater { get; set; }
    }
}
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class ClimateState : IThing
    {
        public string Id => Parent!.Id + "/climate";

        public string? Title => "Climate";

        [ParentThing]
        public TeslaVehicle? Parent { get; private set; }


        [JsonPropertyName("inside_temp"), State]
        public double InsideTemperature { get; set; }

        [JsonPropertyName("outside_temp"), State]
        public double OutsideTemperature { get; set; }

        [JsonPropertyName("allow_cabin_overheat_protection"), State]
        public bool AllowCabinOverheatProtection { get; set; }

        [JsonPropertyName("auto_seat_climate_left"), State]
        public bool AutoSeatClimateLeft { get; set; }

        [JsonPropertyName("auto_seat_climate_right"), State]
        public bool AutoSeatClimateRight { get; set; }

        [JsonPropertyName("battery_heater"), State]
        public bool IsBatteryHeaterOn { get; set; }

        [JsonPropertyName("battery_heater_no_power"), State]
        public object? BatteryHeaterNoPower { get; set; }

        [JsonPropertyName("cabin_overheat_protection"), State]
        public string? CabinOverheatProtection { get; set; }

        [JsonPropertyName("cabin_overheat_protection_actively_cooling"), State]
        public bool CabinOverheatProtectionActivelyCooling { get; set; }

        [JsonPropertyName("climate_keeper_mode"), State]
        public string? ClimateKeeperMode { get; set; }

        [JsonPropertyName("cop_activation_temperature"), State]
        public string? CopActivationTemperature { get; set; }

        [JsonPropertyName("defrost_mode"), State]
        public long DefrostMode { get; set; }

        [JsonPropertyName("driver_temp_setting"), State]
        public double DriverTemperatureSetting { get; set; }

        [JsonPropertyName("fan_status"), State]
        public long FanStatus { get; set; }

        [JsonPropertyName("hvac_auto_request"), State]
        public string? HvacAutoRequest { get; set; }

        [JsonPropertyName("is_auto_conditioning_on"), State]
        public bool IsAutoConditioningOn { get; set; }

        [JsonPropertyName("is_climate_on"), State]
        public bool IsClimateOn { get; set; }

        [JsonPropertyName("is_front_defroster_on"), State]
        public bool IsFrontDefrosterOn { get; set; }

        [JsonPropertyName("is_preconditioning"), State]
        public bool IsPreconditioning { get; set; }

        [JsonPropertyName("is_rear_defroster_on"), State]
        public bool IsRearDefrosterOn { get; set; }


        [JsonPropertyName("left_temp_direction"), State]
        public long LeftTemperatureDirection { get; set; }

        [JsonPropertyName("right_temp_direction"), State]
        public long RightTemperatureDirection { get; set; }


        [JsonPropertyName("max_avail_temp"), State]
        public double MaximumAvailableTemperature { get; set; }

        [JsonPropertyName("min_avail_temp"), State]
        public double MinimumAvailableTemperature { get; set; }

        [JsonPropertyName("passenger_temp_setting"), State]
        public double PassengerTemperatureSetting { get; set; }

        [JsonPropertyName("remote_heater_control_enabled"), State]
        public bool IsRemoteHeaterControlEnabled { get; set; }

        [JsonPropertyName("seat_heater_left"), State]
        public long SeatHeaterLeft { get; set; }

        [JsonPropertyName("seat_heater_rear_center"), State]
        public long SeatHeaterRearCenter { get; set; }

        [JsonPropertyName("seat_heater_rear_left"), State]
        public long SeatHeaterRearLeft { get; set; }

        [JsonPropertyName("seat_heater_rear_right"), State]
        public long SeatHeaterRearRight { get; set; }

        [JsonPropertyName("seat_heater_right"), State]
        public long SeatHeaterRight { get; set; }

        [JsonPropertyName("side_mirror_heaters"), State]
        public bool SideMirrorHeaters { get; set; }

        [JsonPropertyName("steering_wheel_heater"), State]
        public bool SteeringWheelHeater { get; set; }

        [JsonPropertyName("supports_fan_only_cabin_overheat_protection"), State]
        public bool SupportsFanOnlyCabinOverheatProtection { get; set; }

        [JsonPropertyName("timestamp"), State]
        public long Timestamp { get; set; }

        [JsonPropertyName("wiper_blade_heater"), State]
        public bool HasWiperBladeHeater { get; set; }
    }
}
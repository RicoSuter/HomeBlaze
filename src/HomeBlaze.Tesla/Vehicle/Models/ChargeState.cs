using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    // https://tesla-api.timdorr.com/

    public partial class ChargeState
    {
        [JsonPropertyName("battery_heater_on"), State]
        public bool Battery_heater_on { get; set; }

        [JsonPropertyName("battery_level"), State]
        public long Battery_level { get; set; }

        [JsonPropertyName("battery_range"), State]
        public double Battery_range { get; set; }

        [JsonPropertyName("charge_amps"), State]
        public long Charge_amps { get; set; }

        [JsonPropertyName("charge_current_request"), State]
        public long Charge_current_request { get; set; }

        [JsonPropertyName("charge_current_request_max"), State]
        public long Charge_current_request_max { get; set; }

        [JsonPropertyName("charge_enable_request"), State]
        public bool Charge_enable_request { get; set; }

        [JsonPropertyName("charge_energy_added"), State]
        public double Charge_energy_added { get; set; }

        [JsonPropertyName("charge_limit_soc"), State]
        public long Charge_limit_soc { get; set; }

        [JsonPropertyName("charge_limit_soc_max"), State]
        public long Charge_limit_soc_max { get; set; }

        [JsonPropertyName("charge_limit_soc_min"), State]
        public long Charge_limit_soc_min { get; set; }

        [JsonPropertyName("charge_limit_soc_std"), State]
        public long Charge_limit_soc_std { get; set; }

        [JsonPropertyName("charge_miles_added_ideal"), State]
        public double Charge_miles_added_ideal { get; set; }

        [JsonPropertyName("charge_miles_added_rated"), State]
        public double Charge_miles_added_rated { get; set; }

        [JsonPropertyName("charge_port_cold_weather_mode"), State]
        public bool Charge_port_cold_weather_mode { get; set; }

        [JsonPropertyName("charge_port_color"), State]
        public string? Charge_port_color { get; set; }

        [JsonPropertyName("charge_port_door_open"), State]
        public bool Charge_port_door_open { get; set; }

        [JsonPropertyName("charge_port_latch"), State]
        public string? Charge_port_latch { get; set; }

        [JsonPropertyName("charge_rate"), State]
        public double Charge_rate { get; set; }

        [JsonPropertyName("charger_actual_current"), State]
        public long Charger_actual_current { get; set; }

        [JsonPropertyName("charger_phases"), State]
        public long Charger_phases { get; set; }

        [JsonPropertyName("charger_pilot_current"), State]
        public long Charger_pilot_current { get; set; }

        [JsonPropertyName("charger_power"), State]
        public long Charger_power { get; set; }

        [JsonPropertyName("charger_voltage"), State]
        public long Charger_voltage { get; set; }

        [JsonPropertyName("charging_state"), State]
        public string? Charging_state { get; set; }

        [JsonPropertyName("conn_charge_cable"), State]
        public string? Conn_charge_cable { get; set; }

        [JsonPropertyName("est_battery_range"), State]
        public double Est_battery_range { get; set; }

        [JsonPropertyName("fast_charger_brand"), State]
        public string? Fast_charger_brand { get; set; }

        [JsonPropertyName("fast_charger_present"), State]
        public bool Fast_charger_present { get; set; }

        [JsonPropertyName("fast_charger_type"), State]
        public string? Fast_charger_type { get; set; }

        [JsonPropertyName("ideal_battery_range"), State]
        public double Ideal_battery_range { get; set; }

        [JsonPropertyName("managed_charging_active"), State]
        public bool Managed_charging_active { get; set; }

        [JsonPropertyName("managed_charging_start_time"), State]
        public object? Managed_charging_start_time { get; set; }

        [JsonPropertyName("managed_charging_user_canceled"), State]
        public bool Managed_charging_user_canceled { get; set; }

        [JsonPropertyName("max_range_charge_counter"), State]
        public long Max_range_charge_counter { get; set; }

        [JsonPropertyName("minutes_to_full_charge"), State]
        public long Minutes_to_full_charge { get; set; }

        [JsonPropertyName("not_enough_power_to_heat"), State]
        public object? Not_enough_power_to_heat { get; set; }

        [JsonPropertyName("off_peak_charging_enabled"), State]
        public bool Off_peak_charging_enabled { get; set; }

        [JsonPropertyName("off_peak_charging_times"), State]
        public string? Off_peak_charging_times { get; set; }

        [JsonPropertyName("off_peak_hours_end_time"), State]
        public long Off_peak_hours_end_time { get; set; }

        [JsonPropertyName("preconditioning_enabled"), State]
        public bool Preconditioning_enabled { get; set; }

        [JsonPropertyName("preconditioning_times"), State]
        public string? Preconditioning_times { get; set; }

        [JsonPropertyName("scheduled_charging_mode"), State]
        public string? Scheduled_charging_mode { get; set; }

        [JsonPropertyName("scheduled_charging_pending"), State]
        public bool Scheduled_charging_pending { get; set; }

        [JsonPropertyName("scheduled_charging_start_time"), State]
        public long Scheduled_charging_start_time { get; set; }

        [JsonPropertyName("scheduled_charging_start_time_app"), State]
        public long Scheduled_charging_start_time_app { get; set; }

        [JsonPropertyName("scheduled_charging_start_time_minutes"), State]
        public long Scheduled_charging_start_time_minutes { get; set; }

        [JsonPropertyName("scheduled_departure_time"), State]
        public long Scheduled_departure_time { get; set; }

        [JsonPropertyName("scheduled_departure_time_minutes"), State]
        public long Scheduled_departure_time_minutes { get; set; }

        [JsonPropertyName("supercharger_session_trip_planner"), State]
        public bool Supercharger_session_trip_planner { get; set; }

        [JsonPropertyName("time_to_full_charge"), State]
        public double Time_to_full_charge { get; set; }

        [JsonPropertyName("timestamp"), State]
        public long Timestamp { get; set; }

        [JsonPropertyName("trip_charging"), State]
        public bool Trip_charging { get; set; }

        [JsonPropertyName("usable_battery_level"), State]
        public long Usable_battery_level { get; set; }

        [JsonPropertyName("user_charge_enable_request"), State]
        public object? User_charge_enable_request { get; set; }
    }
}
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    // https://tesla-api.timdorr.com/

    public partial class ChargeState : IThing, IIconProvider
    {
        public string Id => Parent!.Id + "/charge";

        public string? Title => "Charge";

        public string IconName => "fa-solid fa-bolt-lightning";

        [ParentThing]
        public TeslaVehicle? Parent { get; private set; }


        [JsonPropertyName("battery_level"), State]
        public long BatteryLevel { get; set; }

        [JsonPropertyName("usable_battery_level"), State]
        public long UsableBatteryLevel { get; set; }

        [JsonPropertyName("battery_range"), State]
        public double BatteryRange { get; set; }

        [JsonPropertyName("ideal_battery_range"), State]
        public decimal IdealBatteryRange { get; set; }

        [JsonPropertyName("est_battery_range"), State]
        public double EstimatedBatteryRange { get; set; }

        [JsonPropertyName("battery_heater_on"), State]
        public bool IsBatteryHeaterOn { get; set; }



        [JsonPropertyName("charge_energy_added"), State(Unit = StateUnit.KiloWatt)]
        public decimal ChargeEnergyAdded { get; set; }

        [JsonPropertyName("charge_amps"), State(Unit = StateUnit.Ampere)]
        public long ChargeAmps { get; set; }

        [JsonPropertyName("charge_current_request"), State]
        public long ChargeCurrentRequest { get; set; }

        [JsonPropertyName("charge_current_request_max"), State]
        public long ChargeCurrentRequestMaximum { get; set; }

        [JsonPropertyName("charge_enable_request"), State]
        public bool HasChargeEnableRequest { get; set; }

        [JsonPropertyName("charge_limit_soc"), State]
        public long ChargeLimitSoc { get; set; }

        [JsonPropertyName("charge_limit_soc_max"), State]
        public long ChargeLimitSocMaximum { get; set; }

        [JsonPropertyName("charge_limit_soc_min"), State]
        public long ChargeLimitSocMinimum { get; set; }

        [JsonPropertyName("charge_limit_soc_std"), State]
        public long ChargeLimitSocStandard { get; set; }

        [JsonPropertyName("charge_miles_added_ideal"), State]
        public decimal ChargeMilesAddedIdeal { get; set; }

        [JsonPropertyName("charge_miles_added_rated"), State]
        public double ChargeMilesAddedRated { get; set; }

        [JsonPropertyName("charge_port_cold_weather_mode"), State]
        public bool IsChargePortInColdWeatherMode { get; set; }

        [JsonPropertyName("charge_port_color"), State]
        public string? ChargePortColor { get; set; }

        [JsonPropertyName("charge_port_door_open"), State]
        public bool ChargePortDoorOpen { get; set; }

        [JsonPropertyName("charge_port_latch"), State]
        public string? ChargePortLatch { get; set; }

        [JsonPropertyName("charge_rate"), State]
        public double ChargeRate { get; set; }



        [JsonPropertyName("minutes_to_full_charge"), State]
        public long MinutesToFullCharge { get; set; }

        [JsonPropertyName("charger_actual_current"), State]
        public long ChargerActualCurrent { get; set; }

        [JsonPropertyName("charger_phases"), State]
        public long? ChargerPhases { get; set; }

        [JsonPropertyName("charger_pilot_current"), State]
        public long ChargerPilotCurrent { get; set; }

        [JsonPropertyName("charger_power"), State]
        public long ChargerPower { get; set; }

        [JsonPropertyName("charger_voltage"), State]
        public long ChargerVoltage { get; set; }

        [JsonPropertyName("charging_state"), State]
        public string? ChargingState { get; set; }

        [JsonPropertyName("conn_charge_cable"), State]
        public string? ConnectionChargeCable { get; set; }



        [JsonPropertyName("fast_charger_brand"), State]
        public string? FastChargerBrand { get; set; }

        [JsonPropertyName("fast_charger_present"), State]
        public bool IsFastChargerPresent { get; set; }

        [JsonPropertyName("fast_charger_type"), State]
        public string? FastChargerType { get; set; }

        [JsonPropertyName("managed_charging_active"), State]
        public bool IsManagedChargingActive { get; set; }

        [JsonPropertyName("managed_charging_start_time"), State]
        public object? ManagedChargingStartTime { get; set; }

        [JsonPropertyName("managed_charging_user_canceled"), State]
        public bool ManagedChargingUserCanceled { get; set; }

        [JsonPropertyName("max_range_charge_counter"), State]
        public long MaxRangeChargeCounter { get; set; }


        [JsonPropertyName("not_enough_power_to_heat"), State]
        public object? HasNotEnoughPowerToHeat { get; set; }

        [JsonPropertyName("off_peak_charging_enabled"), State]
        public bool IsOffPeakChargingEnabled { get; set; }

        [JsonPropertyName("off_peak_charging_times"), State]
        public string? OffPeakChargingTimes { get; set; }

        [JsonPropertyName("off_peak_hours_end_time"), State]
        public long OffPeakHoursEndTime { get; set; }

        [JsonPropertyName("preconditioning_enabled"), State]
        public bool IsPreconditioningEnabled { get; set; }

        [JsonPropertyName("preconditioning_times"), State]
        public string? PreconditioningTimes { get; set; }

        [JsonPropertyName("scheduled_charging_mode"), State]
        public string? ScheduledChargingMode { get; set; }

        [JsonPropertyName("scheduled_charging_pending"), State]
        public bool IsScheduledChargingPending { get; set; }

        [JsonPropertyName("scheduled_charging_start_time"), State]
        public long? ScheduledChargingStartTime { get; set; }

        [JsonPropertyName("scheduled_charging_start_time_app"), State]
        public long ScheduledChargingStartTimeApp { get; set; }

        [JsonPropertyName("scheduled_charging_start_time_minutes"), State]
        public long ScheduledChargingStartTimeMinutes { get; set; }

        [JsonPropertyName("scheduled_departure_time"), State]
        public long? ScheduledDepartureTime { get; set; }

        [JsonPropertyName("scheduled_departure_time_minutes"), State]
        public long ScheduledDepartureTimeMinutes { get; set; }

        [JsonPropertyName("supercharger_session_trip_planner"), State]
        public bool SuperchargerSessionTripPlanner { get; set; }

        [JsonPropertyName("time_to_full_charge"), State]
        public double TimeToFullCharge { get; set; }

        [JsonPropertyName("timestamp"), State]
        public long Timestamp { get; set; }

        [JsonPropertyName("trip_charging"), State]
        public bool? TripCharging { get; set; }

        [JsonPropertyName("user_charge_enable_request"), State]
        public object? UserChargeEnableRequest { get; set; }
    }
}
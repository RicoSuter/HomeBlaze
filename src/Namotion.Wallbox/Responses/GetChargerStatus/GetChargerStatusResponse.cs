using System.Text.Json.Serialization;

using HomeBlaze.Abstractions.Attributes;

namespace Namotion.Wallbox.Responses.GetChargerStatus
{
    public class GetChargerStatusResponse
    {
        [State]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [State(Unit = StateUnit.Ampere)]
        [JsonIgnore]
        public decimal? MinimumAllowedChargingCurrent => 6m;

        [State(Unit = StateUnit.Ampere)]
        [JsonPropertyName("max_available_power"), JsonInclude]
        public decimal? MaximumAllowedChargingCurrent { get; set; }

        [State(Unit = StateUnit.Volt)]
        public decimal? GridVoltage => ChargingPower * 1000m / ChargingSpeed / CurrentMode;

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("user_name")]
        public string? UserName { get; set; }

        [State]
        [JsonPropertyName("car_id")]
        public int CarId { get; set; }

        [State]
        [JsonPropertyName("car_plate")]
        public string? CarPlate { get; set; }

        [JsonPropertyName("depot_price")]
        public decimal DepotPrice { get; set; }

        [JsonPropertyName("last_sync")]
        public string? LastSync { get; set; }

        [State]
        [JsonPropertyName("power_sharing_status")]
        public int PowerSharingStatus { get; set; }

        [State]
        [JsonPropertyName("mid_status")]
        public int MidStatus { get; set; }

        [JsonPropertyName("status_id")]
        public int StatusId { get; set; }

        [State]
        public ChargerStatus Status
        {
            get
            {
                return StatusId switch
                {
                    0 => ChargerStatus.Disconnected,
                    14 => ChargerStatus.Error,
                    15 => ChargerStatus.Error,
                    161 => ChargerStatus.Ready,
                    162 => ChargerStatus.Ready,
                    163 => ChargerStatus.Disconnected,
                    164 => ChargerStatus.Waiting,
                    165 => ChargerStatus.Locked,
                    166 => ChargerStatus.Updating,
                    177 => ChargerStatus.Scheduled,
                    178 => ChargerStatus.Paused,
                    179 => ChargerStatus.Scheduled,
                    180 => ChargerStatus.WaitingForCarDemand,
                    181 => ChargerStatus.WaitingForCarDemand,
                    182 => ChargerStatus.Paused,
                    183 => ChargerStatus.WaitingInQueueByPowerSharing,
                    184 => ChargerStatus.WaitingInQueueByPowerSharing,
                    185 => ChargerStatus.WaitingInQueueByPowerBoost,
                    186 => ChargerStatus.WaitingInQueueByPowerBoost,
                    187 => ChargerStatus.WaitingMidFailed,
                    188 => ChargerStatus.WaitingMidSafetyMarginExceeded,
                    189 => ChargerStatus.WaitingInQueueByEcoSmart,
                    193 => ChargerStatus.Charging,
                    194 => ChargerStatus.Charging,
                    195 => ChargerStatus.Charging,
                    196 => ChargerStatus.Discharging,
                    209 => ChargerStatus.Locked,
                    210 => ChargerStatus.LockedCarConnected,
                    _ => ChargerStatus.Unknown
                };
            }
        }

        [JsonPropertyName("charging_power"), JsonInclude]
        internal decimal ChargingPower { get; set; }

        [JsonPropertyName("depot_name")]
        public string? DepotName { get; set; }

        [State(Unit = StateUnit.Ampere)]
        [JsonPropertyName("charging_speed")]
        public decimal ChargingSpeed { get; set; }

        [JsonPropertyName("added_range")]
        public decimal AddedRange { get; set; }

        [JsonPropertyName("added_energy")]
        public decimal AddedEnergy { get; set; }

        [JsonPropertyName("added_green_energy")]
        public decimal AddedGreenEnergy { get; set; }

        [JsonPropertyName("added_discharged_energy")]
        public decimal AddedDischargedEnergy { get; set; }

        [JsonPropertyName("added_grid_energy")]
        public decimal AddedGridEnergy { get; set; }

        [JsonPropertyName("charging_time")]
        public int ChargingTime { get; set; }

        [JsonPropertyName("finished")]
        public bool Finished { get; set; }

        [JsonPropertyName("cost")]
        public decimal Cost { get; set; }

        [State]
        [JsonPropertyName("current_mode")]
        public int CurrentMode { get; set; }

        [JsonPropertyName("preventive_discharge")]
        public bool PreventiveDischarge { get; set; }

        [State]
        [JsonPropertyName("state_of_charge")]
        public int? StateOfCharge { get; set; }

        [JsonPropertyName("ocpp_status")]
        public int OcppStatus { get; set; }

        [JsonPropertyName("config_data")]
        [ScanForState]
        public ChargerConfiguration? ConfigData { get; set; }
    }
}

using System.Text.Json.Serialization;
using HomeBlaze.Abstractions.Attributes;

namespace Namotion.Wallbox.Responses
{
    public class ChargerStatusResponse
    {
        [State]
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [State(Unit = StateUnit.Ampere)]
        [JsonPropertyName("max_available_power"), JsonInclude]
        public float MaximumAvailableCurrent { get; set; }

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
        public float DepotPrice { get; set; }

        [JsonPropertyName("last_sync")]
        public string? LastSync { get; set; }

        [State]
        [JsonPropertyName("power_sharing_status")]
        public int PowerSharingStatus { get; set; }

        [State]
        [JsonPropertyName("mid_status")]
        public int MidStatus { get; set; }

        [State]
        [JsonPropertyName("status_id")]
        public int StatusId { get; set; }

        [JsonPropertyName("charging_power"), JsonInclude]
        internal decimal ChargingPower { get; set; }

        [JsonPropertyName("depot_name")]
        public string? DepotName { get; set; }

        [JsonPropertyName("charging_speed")]
        public float ChargingSpeed { get; set; }

        [JsonPropertyName("added_range")]
        public float AddedRange { get; set; }

        [JsonPropertyName("added_energy")]
        public float AddedEnergy { get; set; }

        [JsonPropertyName("added_green_energy")]
        public float AddedGreenEnergy { get; set; }

        [JsonPropertyName("added_discharged_energy")]
        public float AddedDischargedEnergy { get; set; }

        [JsonPropertyName("added_grid_energy")]
        public float AddedGridEnergy { get; set; }

        [JsonPropertyName("charging_time")]
        public int ChargingTime { get; set; }

        [JsonPropertyName("finished")]
        public bool Finished { get; set; }

        [JsonPropertyName("cost")]
        public float Cost { get; set; }

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
        public ConfigData? ConfigData { get; set; }
    }
}

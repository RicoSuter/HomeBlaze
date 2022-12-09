using HomeBlaze.Abstractions.Attributes;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class TeslaVehicleData
    {
        [JsonPropertyName("id"), State]
        public long Id { get; set; }

        [JsonPropertyName("user_id"), State]
        public long User_id { get; set; }

        [JsonPropertyName("vehicle_id"), State]
        public long Vehicle_id { get; set; }

        [JsonPropertyName("vin"), State]
        public string? Vin { get; set; }

        [JsonPropertyName("display_name"), State]
        public string? Display_name { get; set; }

        [JsonPropertyName("option_codes"), State]
        public object? Option_codes { get; set; }

        [JsonPropertyName("color"), State]
        public object? Color { get; set; }

        [JsonPropertyName("access_type"), State]
        public string? Access_type { get; set; }

        [JsonPropertyName("tokens"), State]
        public ICollection<string>? Tokens { get; set; }

        [JsonPropertyName("state"), State]
        public string? State { get; set; }

        [JsonPropertyName("in_service"), State]
        public bool In_service { get; set; }

        [JsonPropertyName("id_s"), State]
        public string? Id_s { get; set; }

        [JsonPropertyName("calendar_enabled"), State]
        public bool Calendar_enabled { get; set; }

        [JsonPropertyName("api_version"), State]
        public long Api_version { get; set; }

        [JsonPropertyName("backseat_token"), State]
        public object? Backseat_token { get; set; }

        [JsonPropertyName("backseat_token_updated_at"), State]
        public object? Backseat_token_updated_at { get; set; }

        [JsonPropertyName("charge_state"), State]
        public ChargeState? Charge_state { get; set; }

        [JsonPropertyName("climate_state"), State]
        public ClimateState? Climate_state { get; set; }

        [JsonPropertyName("drive_state"), State]
        public DriveState? Drive_state { get; set; }

        [JsonPropertyName("gui_settings"), State]
        public GuiSettings? Gui_settings { get; set; }

        [JsonPropertyName("vehicle_config"), State]
        public VehicleConfiguration? Vehicle_config { get; set; }

        [JsonPropertyName("vehicle_state"), State]
        public VehicleState? Vehicle_state { get; set; }
    }
}
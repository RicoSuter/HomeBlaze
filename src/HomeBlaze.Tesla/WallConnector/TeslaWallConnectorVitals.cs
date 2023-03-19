using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.WallConnector
{
    public class TeslaWallConnectorVitals
    {
        [JsonPropertyName("contactor_closed")]
        public bool IsContactorClosed { get; set; }

        [JsonPropertyName("vehicle_connected")]
        public bool IsVehicleConnected { get; set; }

        [JsonPropertyName("session_s")]
        public int SessionDuration { get; set; }

        [JsonPropertyName("grid_v")]
        public decimal Grid_v { get; set; }

        [JsonPropertyName("grid_hz")]
        public decimal Grid_hz { get; set; }

        [JsonPropertyName("vehicle_current_a")]
        public double VehicleCurrent { get; set; }

        [JsonPropertyName("currentA_a")]
        public double CurrentA_a { get; set; }

        [JsonPropertyName("currentB_a")]
        public decimal CurrentB_a { get; set; }

        [JsonPropertyName("currentC_a")]
        public decimal CurrentC_a { get; set; }

        [JsonPropertyName("currentN_a")]
        public decimal CurrentN_a { get; set; }

        [JsonPropertyName("voltageA_v")]
        public decimal VoltageA_v { get; set; }

        [JsonPropertyName("voltageB_v")]
        public decimal VoltageB_v { get; set; }

        [JsonPropertyName("voltageC_v")]
        public decimal VoltageC_v { get; set; }

        [JsonPropertyName("relay_coil_v")]
        public double Relay_coil_v { get; set; }

        [JsonPropertyName("pcba_temp_c")]
        public double Pcba_temp_c { get; set; }

        [JsonPropertyName("handle_temp_c")]
        public double Handle_temp_c { get; set; }

        [JsonPropertyName("mcu_temp_c")]
        public double Mcu_temp_c { get; set; }

        [JsonPropertyName("uptime_s")]
        public int Uptime_s { get; set; }

        [JsonPropertyName("input_thermopile_uv")]
        public int Input_thermopile_uv { get; set; }

        [JsonPropertyName("prox_v")]
        public double Prox_v { get; set; }

        [JsonPropertyName("pilot_high_v")]
        public double Pilot_high_v { get; set; }

        [JsonPropertyName("pilot_low_v")]
        public double Pilot_low_v { get; set; }

        [JsonPropertyName("session_energy_wh")]
        public double Session_energy_wh { get; set; }

        [JsonPropertyName("config_status")]
        public int Config_status { get; set; }

        [JsonPropertyName("evse_state")]
        public int Evse_state { get; set; }
    }
}
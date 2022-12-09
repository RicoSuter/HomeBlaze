using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class VehicleState
    {
        private decimal _odometer;

        [JsonPropertyName("api_version"), State]
        public long ApiVersion { get; set; }

        [JsonPropertyName("autopark_state_v2"), State]
        public string? Autopark_state_v2 { get; set; }

        [JsonPropertyName("calendar_supported"), State]
        public bool Calendar_supported { get; set; }

        [JsonPropertyName("car_version"), State]
        public string? Car_version { get; set; }

        [JsonPropertyName("center_display_state"), State]
        public long Center_display_state { get; set; }

        [JsonPropertyName("dashcam_clip_save_available"), State]
        public bool Dashcam_clip_save_available { get; set; }

        [JsonPropertyName("dashcam_state"), State]
        public string? Dashcam_state { get; set; }

        [JsonPropertyName("df"), State]
        public long Df { get; set; }

        [JsonPropertyName("dr"), State]
        public long Dr { get; set; }

        [JsonPropertyName("fd_window"), State]
        public long Fd_window { get; set; }

        [JsonPropertyName("feature_bitmask"), State]
        public string? Feature_bitmask { get; set; }

        [JsonPropertyName("fp_window"), State]
        public long Fp_window { get; set; }

        [JsonPropertyName("ft"), State]
        public long Ft { get; set; }

        [JsonPropertyName("is_user_present"), State]
        public bool Is_user_present { get; set; }

        [JsonPropertyName("locked"), State]
        public bool Locked { get; set; }

        [JsonPropertyName("media_info"), State]
        public MediaInfo? Media_info { get; set; }

        [JsonPropertyName("media_state"), State]
        public MediaState? Media_state { get; set; }

        [JsonPropertyName("notifications_supported"), State]
        public bool Notifications_supported { get; set; }

        [JsonPropertyName("odometer"), State(Unit = StateUnit.Meter)]
        public decimal Odometer
        {
            get => _odometer;
            set => _odometer = value * 1609.344m;
        }

        [JsonPropertyName("parsed_calendar_supported"), State]
        public bool Parsed_calendar_supported { get; set; }

        [JsonPropertyName("pf"), State]
        public long Pf { get; set; }

        [JsonPropertyName("pr"), State]
        public long Pr { get; set; }

        [JsonPropertyName("rd_window"), State]
        public long Rd_window { get; set; }

        [JsonPropertyName("remote_start"), State]
        public bool Remote_start { get; set; }

        [JsonPropertyName("remote_start_enabled"), State]
        public bool Remote_start_enabled { get; set; }

        [JsonPropertyName("remote_start_supported"), State]
        public bool Remote_start_supported { get; set; }

        [JsonPropertyName("rp_window"), State]
        public long Rp_window { get; set; }

        [JsonPropertyName("rt"), State]
        public long Rt { get; set; }

        [JsonPropertyName("santa_mode"), State]
        public long Santa_mode { get; set; }

        [JsonPropertyName("sentry_mode"), State]
        public bool Sentry_mode { get; set; }

        [JsonPropertyName("sentry_mode_available"), State]
        public bool Sentry_mode_available { get; set; }

        [JsonPropertyName("service_mode"), State]
        public bool Service_mode { get; set; }

        [JsonPropertyName("service_mode_plus"), State]
        public bool Service_mode_plus { get; set; }

        [JsonPropertyName("software_update"), State]
        public SoftwareUpdate? Software_update { get; set; }

        [JsonPropertyName("speed_limit_mode"), State]
        public SpeedLimitMode? Speed_limit_mode { get; set; }

        [JsonPropertyName("timestamp"), State]
        public long Timestamp { get; set; }

        [JsonPropertyName("tpms_hard_warning_fl"), State]
        public bool Tpms_hard_warning_fl { get; set; }

        [JsonPropertyName("tpms_hard_warning_fr"), State]
        public bool Tpms_hard_warning_fr { get; set; }

        [JsonPropertyName("tpms_hard_warning_rl"), State]
        public bool Tpms_hard_warning_rl { get; set; }

        [JsonPropertyName("tpms_hard_warning_rr"), State]
        public bool Tpms_hard_warning_rr { get; set; }

        [JsonPropertyName("tpms_last_seen_pressure_time_fl"), State]
        public long Tpms_last_seen_pressure_time_fl { get; set; }

        [JsonPropertyName("tpms_last_seen_pressure_time_fr"), State]
        public long Tpms_last_seen_pressure_time_fr { get; set; }

        [JsonPropertyName("tpms_last_seen_pressure_time_rl"), State]
        public long Tpms_last_seen_pressure_time_rl { get; set; }

        [JsonPropertyName("tpms_last_seen_pressure_time_rr"), State]
        public long Tpms_last_seen_pressure_time_rr { get; set; }

        [JsonPropertyName("tpms_pressure_fl"), State]
        public double TirePressureFrontLeft { get; set; }

        [JsonPropertyName("tpms_pressure_fr"), State]
        public double TirePressureFrontRight { get; set; }

        [JsonPropertyName("tpms_pressure_rl"), State]
        public double TirePressureRearLeft { get; set; }

        [JsonPropertyName("tpms_pressure_rr"), State]
        public double TirePressureRearRight { get; set; }

        [JsonPropertyName("tpms_rcp_front_value"), State]
        public double Tpms_rcp_front_value { get; set; }

        [JsonPropertyName("tpms_rcp_rear_value"), State]
        public double Tpms_rcp_rear_value { get; set; }

        [JsonPropertyName("tpms_soft_warning_fl"), State]
        public bool Tpms_soft_warning_fl { get; set; }

        [JsonPropertyName("tpms_soft_warning_fr"), State]
        public bool Tpms_soft_warning_fr { get; set; }

        [JsonPropertyName("tpms_soft_warning_rl"), State]
        public bool Tpms_soft_warning_rl { get; set; }

        [JsonPropertyName("tpms_soft_warning_rr"), State]
        public bool Tpms_soft_warning_rr { get; set; }

        [JsonPropertyName("valet_mode"), State]
        public bool Valet_mode { get; set; }

        [JsonPropertyName("valet_pin_needed"), State]
        public bool Valet_pin_needed { get; set; }

        [JsonPropertyName("vehicle_name"), State]
        public string? Vehicle_name { get; set; }

        [JsonPropertyName("webcam_available"), State]
        public bool Webcam_available { get; set; }
    }
}
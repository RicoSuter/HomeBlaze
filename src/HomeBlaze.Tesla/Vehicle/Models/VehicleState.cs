using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class VehicleState : IThing, IIconProvider
    {
        private decimal _odometer;

        public string Id => Parent?.Id + "/vehicle";

        public string? Title => "Vehicle";

        public string IconName => "fa-solid fa-car";

        [ParentThing]
        public TeslaVehicle? Parent { get; private set; }


        [JsonPropertyName("api_version"), State]
        public long ApiVersion { get; set; }

        [JsonPropertyName("autopark_state_v2"), State]
        public string? AutoparkStateV2 { get; set; }

        [JsonPropertyName("calendar_supported"), State]
        public bool IsCalendarSupported { get; set; }

        [JsonPropertyName("car_version"), State]
        public string? CarVersion { get; set; }

        [JsonPropertyName("center_display_state"), State]
        public long CenterDisplayState { get; set; }

        [JsonPropertyName("dashcam_clip_save_available"), State]
        public bool DashcamClipSaveAvailable { get; set; }

        [JsonPropertyName("dashcam_state"), State]
        public string? DashcamState { get; set; }

        [JsonPropertyName("fd_window"), State]
        public long FdWindow { get; set; }

        [JsonPropertyName("feature_bitmask"), State]
        public string? FeatureBitmask { get; set; }

        [JsonPropertyName("fp_window"), State]
        public long FpWindow { get; set; }

        [JsonPropertyName("is_user_present"), State]
        public bool IsUserPresent { get; set; }

        [JsonPropertyName("locked"), State]
        public bool IsLocked { get; set; }

        [JsonPropertyName("media_info")]
        public MediaInfo? MediaInfo { get; set; }

        [JsonPropertyName("media_state")]
        public MediaState? MediaState { get; set; }

        [JsonPropertyName("notifications_supported"), State]
        public bool AreNotificationsSupported { get; set; }

        [JsonPropertyName("odometer"), State(Unit = StateUnit.Meter)]
        public decimal Odometer
        {
            get => _odometer;
            set => _odometer = value * 1609.344m;
        }

        [JsonPropertyName("parsed_calendar_supported"), State]
        public bool IsParsedCalendarSupported { get; set; }

        [JsonPropertyName("rd_window"), State]
        public long RdWindow { get; set; }

        [JsonPropertyName("remote_start"), State]
        public bool IsRemoteStart { get; set; }

        [JsonPropertyName("remote_start_enabled"), State]
        public bool IsRemoteStartEnabled { get; set; }

        [JsonPropertyName("remote_start_supported"), State]
        public bool IsRemoteStartSupported { get; set; }

        [JsonPropertyName("rp_window"), State]
        public long RpWindow { get; set; }

        [JsonPropertyName("santa_mode"), State]
        public long SantaMode { get; set; }

        [JsonPropertyName("sentry_mode"), State]
        public bool IsSentryModeOn { get; set; }

        [JsonPropertyName("sentry_mode_available"), State]
        public bool IsSentryModeAvailable { get; set; }

        [JsonPropertyName("service_mode"), State]
        public bool IsInServiceMode { get; set; }

        [JsonPropertyName("service_mode_plus"), State]
        public bool IsInServiceModePlus { get; set; }

        [JsonPropertyName("software_update")]
        public SoftwareUpdate? SoftwareUpdate { get; set; }

        [JsonPropertyName("speed_limit_mode")]
        public SpeedLimitMode? SpeedLimitMode { get; set; }

        [JsonPropertyName("timestamp"), State]
        public long Timestamp { get; set; }

        [JsonPropertyName("tpms_hard_warning_fl"), State]
        public bool TpmsHardWarningFrontLeft { get; set; }

        [JsonPropertyName("tpms_hard_warning_fr"), State]
        public bool TpmsHardWarningFrontRight { get; set; }

        [JsonPropertyName("tpms_hard_warning_rl"), State]
        public bool TpmsHardWarningRearLeft { get; set; }

        [JsonPropertyName("tpms_hard_warning_rr"), State]
        public bool TpmsHardWarningRearRight { get; set; }

        [JsonPropertyName("tpms_last_seen_pressure_time_fl"), State]
        public long TpmsLastSeenPressureTimeFrontLeft { get; set; }

        [JsonPropertyName("tpms_last_seen_pressure_time_fr"), State]
        public long TpmsLastSeenPressureTimeFrontRight { get; set; }

        [JsonPropertyName("tpms_last_seen_pressure_time_rl"), State]
        public long TpmsLastSeenPressureTimeRearLeft { get; set; }

        [JsonPropertyName("tpms_last_seen_pressure_time_rr"), State]
        public long TpmsLastSeenPressureTimeRearRight { get; set; }

        [JsonPropertyName("tpms_pressure_fl"), State]
        public double TirePressureFrontLeft { get; set; }

        [JsonPropertyName("tpms_pressure_fr"), State]
        public double TirePressureFrontRight { get; set; }

        [JsonPropertyName("tpms_pressure_rl"), State]
        public double TirePressureRearLeft { get; set; }

        [JsonPropertyName("tpms_pressure_rr"), State]
        public double TirePressureRearRight { get; set; }

        [JsonPropertyName("tpms_rcp_front_value"), State]
        public double TpmsRcpFrontValue { get; set; }

        [JsonPropertyName("tpms_rcp_rear_value"), State]
        public double TpmsRcpRearValue { get; set; }

        [JsonPropertyName("tpms_soft_warning_fl"), State]
        public bool TpmsSoftWarningFrontLeft { get; set; }

        [JsonPropertyName("tpms_soft_warning_fr"), State]
        public bool TpmsSoftWarningFrontRight { get; set; }

        [JsonPropertyName("tpms_soft_warning_rl"), State]
        public bool TpmsSoftWarningRearLeft { get; set; }

        [JsonPropertyName("tpms_soft_warning_rr"), State]
        public bool TpmsSoftWarningRearRight { get; set; }

        [JsonPropertyName("valet_mode"), State]
        public bool IsInValetMode { get; set; }

        [JsonPropertyName("valet_pin_needed"), State]
        public bool IsValetPinNeeded { get; set; }

        [JsonPropertyName("vehicle_name"), State]
        public string? VehicleName { get; set; }

        [JsonPropertyName("webcam_available"), State]
        public bool IsWebcamAvailable { get; set; }



        [JsonPropertyName("df"), State]
        public long Df { get; set; }

        [JsonPropertyName("dr"), State]
        public long Dr { get; set; }

        [JsonPropertyName("ft"), State]
        public long Ft { get; set; }

        [JsonPropertyName("pf"), State]
        public long Pf { get; set; }

        [JsonPropertyName("pr"), State]
        public long Pr { get; set; }

        [JsonPropertyName("rt"), State]
        public long Rt { get; set; }
    }
}
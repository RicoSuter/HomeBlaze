using System.Text.Json.Serialization;

namespace Namotion.Wallbox.Responses
{
    public class ConfigData
    {
        [JsonPropertyName("charger_id")]
        public int ChargerId { get; set; }

        [JsonPropertyName("uid")]
        public string? Uid { get; set; }

        [JsonPropertyName("serial_number")]
        public string? SerialNumber { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("locked")]
        public int Locked { get; set; }

        [JsonPropertyName("auto_lock")]
        public int AutoLock { get; set; }

        [JsonPropertyName("auto_lock_time")]
        public int AutoLockTime { get; set; }

        [JsonPropertyName("multiuser")]
        public int Multiuser { get; set; }

        [JsonPropertyName("max_charging_current")]
        public int MaxChargingCurrent { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }

        [JsonPropertyName("icp_max_current")]
        public int IcpMaxCurrent { get; set; }

        [JsonPropertyName("grid_type")]
        public int GridType { get; set; }

        [JsonPropertyName("energy_price")]
        public float EnergyPrice { get; set; }

        [JsonPropertyName("energyCost")]
        public EnergyCost? EnergyCost { get; set; }

        [JsonPropertyName("unlock_user_id")]
        public object? UnlockUserId { get; set; }

        [JsonPropertyName("power_sharing_config")]
        public int PowerSharingConfig { get; set; }

        [JsonPropertyName("purchased_power")]
        public float PurchasedPower { get; set; }

        [JsonPropertyName("show_name")]
        public int ShowName { get; set; }

        [JsonPropertyName("show_lastname")]
        public int ShowLastname { get; set; }

        [JsonPropertyName("show_email")]
        public int ShowEmail { get; set; }

        [JsonPropertyName("show_profile")]
        public int ShowProfile { get; set; }

        [JsonPropertyName("show_default_user")]
        public int ShowDefaultUser { get; set; }

        [JsonPropertyName("gesture_status")]
        public int GestureStatus { get; set; }

        [JsonPropertyName("home_sharing")]
        public int HomeSharing { get; set; }

        [JsonPropertyName("dca_status")]
        public int DcaStatus { get; set; }

        [JsonPropertyName("connection_type")]
        public int ConnectionType { get; set; }

        [JsonPropertyName("max_available_current")]
        public int MaxAvailableCurrent { get; set; }

        [JsonPropertyName("live_refresh_time")]
        public int LiveRefreshTime { get; set; }

        [JsonPropertyName("update_refresh_time")]
        public int UpdateRefreshTime { get; set; }

        [JsonPropertyName("owner_id")]
        public int OwnerId { get; set; }

        [JsonPropertyName("remote_action")]
        public int RemoteAction { get; set; }

        [JsonPropertyName("rfid_type")]
        public string? RfidType { get; set; }

        [JsonPropertyName("charger_has_image")]
        public int ChargerHasImage { get; set; }

        [JsonPropertyName("sha256_charger_image")]
        public string? Sha256ChargerImage { get; set; }

        [JsonPropertyName("plan")]
        public Plan? Plan { get; set; }

        [JsonPropertyName("sync_timestamp")]
        public int SyncTimestamp { get; set; }

        [JsonPropertyName("currency")]
        public Currency? Currency { get; set; }

        [JsonPropertyName("charger_load_type")]
        public string? ChargerLoadType { get; set; }

        [JsonPropertyName("contract_charging_available")]
        public bool ContractChargingAvailable { get; set; }

        [JsonPropertyName("country")]
        public Country? Country { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }

        [JsonPropertyName("part_number")]
        public string? PartNumber { get; set; }

        [JsonPropertyName("software")]
        public Software? Software { get; set; }

        [JsonPropertyName("available")]
        public int Available { get; set; }

        [JsonPropertyName("operation_mode")]
        public string? OperationMode { get; set; }

        [JsonPropertyName("ocpp_ready")]
        public string? OcppReady { get; set; }

        [JsonPropertyName("tariffs")]
        public object[]? Tariffs { get; set; }

        [JsonPropertyName("mid_enabled")]
        public int MidEnabled { get; set; }

        [JsonPropertyName("mid_margin")]
        public int MidMargin { get; set; }

        [JsonPropertyName("mid_margin_unit")]
        public int MidMarginUnit { get; set; }

        [JsonPropertyName("mid_serial_number")]
        public string? MidSerialNumber { get; set; }

        [JsonPropertyName("mid_status")]
        public int MidStatus { get; set; }

        [JsonPropertyName("session_segment_length")]
        public int SessionSegmentLength { get; set; }

        [JsonPropertyName("group_id")]
        public int GroupId { get; set; }

        [JsonPropertyName("user_socket_locking")]
        public int UserSocketLocking { get; set; }

        [JsonPropertyName("sim_iccid")]
        public string? SimIccid { get; set; }

        [JsonPropertyName("ecosmart")]
        public EcoSmart? Ecosmart { get; set; }
    }




}

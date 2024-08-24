using System.Collections.ObjectModel;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class TeslaVehicleItem
    {
        //[System.Text.Json.Serialization.JsonPropertyName("id")]
        //public string? Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("vehicle_id")]
        public long? VehicleId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("vin")]
        public string? Vin { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("option_codes")]
        public object? OptionCodes { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("color")]
        public object? Color { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("access_type")]
        public string? AccessType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("tokens")]
        public System.Collections.Generic.ICollection<string> Tokens { get; set; } = new Collection<string>();

        [System.Text.Json.Serialization.JsonPropertyName("state")]
        public string? State { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("in_service")]
        public bool InService { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("id_s")]
        public string? IdString { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("calendar_enabled")]
        public bool IsCalendarEnabled { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("api_version")]
        public int ApiVersion { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("backseat_token")]
        public object? BackseatToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("backseat_token_updated_at")]
        public object? BackseatTokenUpdatedAt { get; set; }
    }
}
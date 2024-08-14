using System.Text.Json.Serialization;
using System;

namespace Namotion.Wallbox.Responses.GetChargingSessions
{
    public class ChargingSessionAttributes
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("start_time")]
        public long StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public long EndTime { get; set; }

        [JsonPropertyName("charging_time")]
        public int ChargingTime { get; set; }

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("user_uid")]
        public required string UserUid { get; set; }

        [JsonPropertyName("user_name")]
        public required string UserName { get; set; }

        [JsonPropertyName("user_surname")]
        public required string UserSurname { get; set; }

        [JsonPropertyName("user_email")]
        public required string UserEmail { get; set; }

        [JsonPropertyName("charger_id")]
        public int ChargerId { get; set; }

        [JsonPropertyName("charger_name")]
        public required string ChargerName { get; set; }

        [JsonPropertyName("group_id")]
        public int GroupId { get; set; }

        [JsonPropertyName("location_id")]
        public int LocationId { get; set; }

        [JsonPropertyName("location_name")]
        public required string LocationName { get; set; }

        [JsonPropertyName("energy")]
        public decimal Energy { get; set; }

        [JsonPropertyName("mid_energy")]
        public decimal MidEnergy { get; set; }

        [JsonPropertyName("energy_price")]
        public decimal EnergyPrice { get; set; }

        [JsonPropertyName("currency_code")]
        public required string CurrencyCode { get; set; }

        [JsonPropertyName("session_type")]
        public required string SessionType { get; set; }

        [JsonPropertyName("application_fee_percentage")]
        public int ApplicationFeePercentage { get; set; }

        [JsonPropertyName("user_avatar")]
        public required string UserAvatar { get; set; }

        [JsonPropertyName("order_uid")]
        public required string OrderUid { get; set; }

        [JsonPropertyName("rate_price")]
        public decimal? RatePrice { get; set; }

        [JsonPropertyName("rate_variable_type")]
        public required string RateVariableType { get; set; }

        [JsonPropertyName("order_energy")]
        public decimal? OrderEnergy { get; set; }

        [JsonPropertyName("access_price")]
        public decimal? AccessPrice { get; set; }

        [JsonPropertyName("fee_amount")]
        public decimal? FeeAmount { get; set; }

        [JsonPropertyName("total")]
        public decimal? Total { get; set; }

        [JsonPropertyName("subtotal")]
        public decimal? Subtotal { get; set; }

        [JsonPropertyName("tax_amount")]
        public decimal? TaxAmount { get; set; }

        [JsonPropertyName("tax_percentage")]
        public decimal? TaxPercentage { get; set; }

        [JsonPropertyName("total_cost")]
        public decimal TotalCost { get; set; }

        [JsonPropertyName("public_charge_uid")]
        public required string PublicChargeUid { get; set; }

        [JsonPropertyName("organization_uid")]
        public required string OrganizationUid { get; set; }

        [JsonPropertyName("charger_uid")]
        public required string ChargerUid { get; set; }

        [JsonPropertyName("location_uid")]
        public required string LocationUid { get; set; }

        // Optional: Add methods to convert Unix timestamps to DateTime objects
        public DateTime GetStartTimeAsDateTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(StartTime).DateTime;
        }

        public DateTime GetEndTimeAsDateTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(EndTime).DateTime;
        }
    }
}
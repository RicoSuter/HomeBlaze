using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public class TeslaVehicleDataResponse
    {
        [JsonPropertyName("response")]
        public TeslaVehicleData? Response { get; set; }
    }
}
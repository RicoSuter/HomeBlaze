using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public class TeslaVehicleDataResponse
    {
        [JsonPropertyName("response")]
        public TeslaVehicleData? Data { get; set; }
    }
}
using System.Collections.ObjectModel;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class Vehicles
    {
        [System.Text.Json.Serialization.JsonPropertyName("response")]
        public System.Collections.Generic.ICollection<TeslaVehicleItem> Response { get; set; } = new Collection<TeslaVehicleItem>();

        [System.Text.Json.Serialization.JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
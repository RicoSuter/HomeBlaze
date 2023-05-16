using System.Collections.ObjectModel;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    [System.CodeDom.Compiler.GeneratedCode("NJsonSchema", "10.6.9.0 (Newtonsoft.Json v13.0.0.0)")]
    public partial class Vehicles
    {
        [System.Text.Json.Serialization.JsonPropertyName("response")]
        public System.Collections.Generic.ICollection<TeslaVehicleItem> Response { get; set; } = new Collection<TeslaVehicleItem>();

        [System.Text.Json.Serialization.JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
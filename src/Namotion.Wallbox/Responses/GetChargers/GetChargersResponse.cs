using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Namotion.Wallbox.Responses.GetChargers
{
    public class GetChargersResponse
    {
        [JsonPropertyName("data")]
        public List<ChargersData>? Data { get; set; }
    }
}

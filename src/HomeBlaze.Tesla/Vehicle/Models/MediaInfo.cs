using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class MediaInfo
    {
        [JsonPropertyName("audio_volume"), State]
        public double Audio_volume { get; set; }

        [JsonPropertyName("audio_volume_increment"), State]
        public double Audio_volume_increment { get; set; }

        [JsonPropertyName("audio_volume_max"), State]
        public double Audio_volume_max { get; set; }
    }
}
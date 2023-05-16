using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.Vehicle.Models
{
    public partial class MediaInfo
    {
        [JsonPropertyName("audio_volume"), State]
        public double AudioVolume { get; set; }

        [JsonPropertyName("audio_volume_increment"), State]
        public double AudioVolumeIncrement { get; set; }

        [JsonPropertyName("audio_volume_max"), State]
        public double AudioVolumeMaximum { get; set; }
    }
}
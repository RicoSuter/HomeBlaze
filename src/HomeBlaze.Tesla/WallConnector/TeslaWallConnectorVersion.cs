using System.Text.Json.Serialization;

namespace HomeBlaze.Tesla.WallConnector
{
    public class TeslaWallConnectorVersion
    {
        [JsonPropertyName("firmware_version")]
        public string? FirmwareVersion { get; set; }

        [JsonPropertyName("part_number")]
        public string? PartNumber { get; set; }

        [JsonPropertyName("serial_number")]
        public string? SerialNumber { get; set; }
    }
}
using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;

namespace HomeBlaze.MyStrom.Model
{
    // Docs: https://developer.nuki.io/page/nuki-bridge-http-api-1-12/4#heading--doorsensor-states

    internal class MyStromSwitchInformation
    {
        [State("MacAddress"), JsonPropertyName("mac")]
        public string? MacAddress { get; set; }

        [State("DeviceName"), JsonPropertyName("name")]
        public string? Name { get; set; }

        [State("DeviceVersion"), JsonPropertyName("version")]
        public string? Version { get; set; }
    }
}

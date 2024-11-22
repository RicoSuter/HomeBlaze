using System.Collections.Generic;
using System.Text.Json.Serialization;

using HomeBlaze.Abstractions.Attributes;

using Namotion.Proxy;

namespace Namotion.Shelly
{
    [GenerateProxy]
    public partial class ShellyInformation
    {
        /// <summary>
        /// Gets the ID of the device.
        /// </summary>
        [JsonPropertyName("id")]
        [State]
        public partial string? Id { get; internal set; }

        /// <summary>
        /// Gets the name of the device.
        /// </summary>
        [JsonPropertyName("name")]
        [State]
        public partial string? Name { get; internal set; }

        /// <summary>
        /// Gets the MAC address of the device.
        /// </summary>
        [JsonPropertyName("mac")]
        [State]
        public partial string? MacAddress { get; internal set; }

        /// <summary>
        /// Gets the slot of the device.
        /// </summary>
        [JsonPropertyName("slot")]
        [State]
        public partial int? Slot { get; internal set; }

        /// <summary>
        /// Gets the model of the device.
        /// </summary>
        [JsonPropertyName("model")]
        [State]
        public partial string? Model { get; internal set; }

        /// <summary>
        /// Gets the generation of the device.
        /// </summary>
        [JsonPropertyName("gen")]
        [State]
        public partial int? Generation { get; internal set; }

        /// <summary>
        /// Gets the firmware identifier of the device.
        /// </summary>
        [JsonPropertyName("fw_id")]
        [State]
        public partial string? FirmwareIdentifier { get; internal set; }

        /// <summary>
        /// Gets the version of the firmware.
        /// </summary>
        [JsonPropertyName("ver")]
        [State]
        public partial string? Version { get; internal set; }

        /// <summary>
        /// Gets the application name.
        /// </summary>
        [JsonPropertyName("app")]
        [State]
        public partial string? Application { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether authentication is enabled.
        /// </summary>
        [JsonPropertyName("auth_en")]
        [State]
        public partial bool? IsAuthenticationEnabled { get; internal set; }

        /// <summary>
        /// Gets the authentication domain.
        /// </summary>
        [JsonPropertyName("auth_domain")]
        [State]
        public partial string? AuthenticationDomain { get; internal set; }

        /// <summary>
        /// Gets the profile of the device.
        /// </summary>
        [JsonPropertyName("profile")]
        [State]
        public partial string? Profile { get; internal set; }

        /// <summary>
        /// Gets the extension data for the device.
        /// </summary>
        [JsonExtensionData]
        public partial Dictionary<string, object>? ExtensionData { get; internal set; }
    }
}

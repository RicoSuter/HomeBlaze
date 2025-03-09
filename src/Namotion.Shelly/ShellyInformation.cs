using System.Collections.Generic;
using System.Text.Json.Serialization;

using HomeBlaze.Abstractions.Attributes;

using Namotion.Interceptor.Attributes;

namespace Namotion.Shelly;

[InterceptorSubject]
public partial class ShellyInformation
{
    /// <summary>
    /// Gets the ID of the device.
    /// </summary>
    [State]
    [JsonPropertyName("id")]
    public partial string? Id { get; init; }

    /// <summary>
    /// Gets the name of the device.
    /// </summary>
    [State]
    [JsonPropertyName("name")]
    public partial string? Name { get; init; }

    /// <summary>
    /// Gets the MAC address of the device.
    /// </summary>
    [State]
    [JsonPropertyName("mac")]
    public partial string? MacAddress { get; init; }

    /// <summary>
    /// Gets the slot of the device.
    /// </summary>
    [State]
    [JsonPropertyName("slot")]
    public partial int? Slot { get; init; }

    /// <summary>
    /// Gets the model of the device.
    /// </summary>
    [State]
    [JsonPropertyName("model")]
    public partial string? Model { get; init; }

    /// <summary>
    /// Gets the generation of the device.
    /// </summary>
    [State]
    [JsonPropertyName("gen")]
    public partial int? Generation { get; init; }

    /// <summary>
    /// Gets the firmware identifier of the device.
    /// </summary>
    [State]
    [JsonPropertyName("fw_id")]
    public partial string? FirmwareIdentifier { get; init; }

    /// <summary>
    /// Gets the version of the firmware.
    /// </summary>
    [State]
    [JsonPropertyName("ver")]
    public partial string? Version { get; init; }

    /// <summary>
    /// Gets the application name.
    /// </summary>
    [State]
    [JsonPropertyName("app")]
    public partial string? Application { get; init; }

    /// <summary>
    /// Gets a value indicating whether authentication is enabled.
    /// </summary>
    [State]
    [JsonPropertyName("auth_en")]
    public partial bool? IsAuthenticationEnabled { get; init; }

    /// <summary>
    /// Gets the authentication domain.
    /// </summary>
    [State]
    [JsonPropertyName("auth_domain")]
    public partial string? AuthenticationDomain { get; init; }

    /// <summary>
    /// Gets the profile of the device.
    /// </summary>
    [State]
    [JsonPropertyName("profile")]
    public partial string? Profile { get; init; }

    /// <summary>
    /// Gets the extension data for the device.
    /// </summary>
    [JsonExtensionData]
    public partial Dictionary<string, object>? ExtensionData { get; init; }
}
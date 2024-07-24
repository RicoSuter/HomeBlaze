using HomeBlaze.Abstractions.Attributes;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Namotion.Shelly;

public class ShellyInformation
{
    [JsonPropertyName("name"), State]
    public string? Name { get; set; }

    [JsonPropertyName("id"), State]
    public string? Identifier { get; set; }

    [JsonPropertyName("mac"), State]
    public string? MacAddress { get; set; }

    [JsonPropertyName("slot"), State]
    public int? Slot { get; set; }

    [JsonPropertyName("model"), State]
    public string? Model { get; set; }

    [JsonPropertyName("gen"), State]
    public int? Generation { get; set; }

    [JsonPropertyName("fw_id"), State]
    public string? FirmwareIdentifier { get; set; }

    [JsonPropertyName("ver"), State]
    public string? Version { get; set; }

    [JsonPropertyName("app"), State]
    public string? Application { get; set; }

    [JsonPropertyName("auth_en"), State]
    public bool? IsAuthenticationEnabled { get; set; }

    [JsonPropertyName("auth_domain"), State]
    public string? AuthenticationDomain { get; set; }

    [JsonPropertyName("profile"), State]
    public string? Profile { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}

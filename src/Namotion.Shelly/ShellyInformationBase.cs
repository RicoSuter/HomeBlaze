using System.Collections.Generic;
using System.Text.Json.Serialization;

using HomeBlaze.Abstractions.Attributes;

using Namotion.Proxy;

namespace Namotion.Shelly
{
    [GenerateProxy]
    public class ShellyInformationBase
    {
        [JsonPropertyName("name"), State]
        public virtual string? Name { get; set; }

        [JsonPropertyName("id"), State]
        public virtual string? Identifier { get; set; }

        [JsonPropertyName("mac"), State]
        public virtual string? MacAddress { get; set; }

        [JsonPropertyName("slot"), State]
        public virtual int? Slot { get; set; }

        [JsonPropertyName("model"), State]
        public virtual string? Model { get; set; }

        [JsonPropertyName("gen"), State]
        public virtual int? Generation { get; set; }

        [JsonPropertyName("fw_id"), State]
        public virtual string? FirmwareIdentifier { get; set; }

        [JsonPropertyName("ver"), State]
        public virtual string? Version { get; set; }

        [JsonPropertyName("app"), State]
        public virtual string? Application { get; set; }

        [JsonPropertyName("auth_en"), State]
        public virtual bool? IsAuthenticationEnabled { get; set; }

        [JsonPropertyName("auth_domain"), State]
        public virtual string? AuthenticationDomain { get; set; }

        [JsonPropertyName("profile"), State]
        public virtual string? Profile { get; set; }

        [JsonExtensionData]
        public virtual Dictionary<string, object>? ExtensionData { get; set; }
    }
}

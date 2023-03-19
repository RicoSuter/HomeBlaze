using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace HomeBlaze.Gardena
{
    public class GardenaIrrigationControl : GardenaDevice, IIconProvider, ILastUpdatedProvider, IConnectedThing
    {
        public override string? Id => GardenaId != null ?
           "gardena.irrigationcontrol." + GardenaId :
           null;

        public string IconName => "fas fa-water";

        public GardenaLocation Location { get; internal set; }

        public DateTimeOffset? LastUpdated { get; internal set; }

        [State]
        public GardenaDevice[] Valves { get; internal set; } = Array.Empty<GardenaDevice>();

        internal override IEnumerable<GardenaDevice> Children => Valves;

        [State]
        public bool IsConnected => RfLinkState == "ONLINE";

        [State]
        public override string? Title { get; set; }

        [State]
        public string? Serial { get; private set; }

        [State]
        public string? ModelType { get; private set; }

        [State]
        public string? BatteryState { get; private set; }

        [State]
        public string? RfLinkState { get; private set; }

        [State]
        public decimal? RfLinkLevel { get; private set; }

        public GardenaIrrigationControl(GardenaLocation location, JObject data)
        {
            Location = location;
            UpdateCommon(data);
        }

        internal override GardenaDevice Update(JObject data)
        {
            return this;
        }

        internal override GardenaDevice UpdateCommon(JObject data)
        {
            GardenaId = data?["id"]?.Value<string>();

            Title = data?["attributes"]?["name"]?["value"]?.Value<string>() ?? Title;
            BatteryState = data?["attributes"]?["batteryState"]?["value"]?.Value<string>() ?? BatteryState;
            RfLinkLevel = data?["attributes"]?["rfLinkLevel"]?["value"]?.Value<decimal>() ?? RfLinkLevel;
            Serial = data?["attributes"]?["serial"]?["value"]?.Value<string>() ?? Serial;
            ModelType = data?["attributes"]?["modelType"]?["value"]?.Value<string>() ?? ModelType;
            RfLinkState = data?["attributes"]?["rfLinkState"]?["value"]?.Value<string>() ?? RfLinkState;

            LastUpdated = DateTimeOffset.Now; // TODO: Use last updated for every value
            return this;
        }
    }
}

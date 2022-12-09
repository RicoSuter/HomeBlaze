using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Presentation;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Gardena
{
    public class GardenaValve : GardenaDevice, IIconProvider, ILastUpdatedProvider, IConnectedDevice
    {
        public override string? Id => GardenaId != null ?
           "gardena.valve." + GardenaId :
           null;

        public string IconName => "fas fa-tint";

        [JsonIgnore]
        public GardenaLocation Location { get; internal set; }

        public DateTimeOffset? LastUpdated { get; internal set; }

        [State(IsSignal = true)]
        public bool IsConnected => State == "OK";

        [State(IsSignal = true)]
        public bool? IsValveOpen => Activity != "CLOSED";

        [State]
        public int? ValveIndex { get; private set; }

        [State]
        public override string? Title { get; set; }

        [State]
        public string? Activity { get; private set; }

        [State]
        public string? State { get; private set; }

        [State]
        public string? LastErrorCode { get; private set; }

        public GardenaValve(GardenaLocation location, JObject data)
        {
            Location = location;
            Update(data);
        }

        [Operation]
        public async Task RefreshAsync(CancellationToken cancellationToken)
        {
            await Location.RefreshAsync(cancellationToken);
        }

        [Operation]
        public async Task OpenValveAsync(int secondsToOpen, CancellationToken cancellationToken)
        {
            if (Location?.GardenaClient != null && GardenaId != null)
            {
                var json = @"{
    ""data"": {
        ""type"": ""VALVE_CONTROL"",
        ""id"": """ + Guid.NewGuid().ToString() + @""",
        ""attributes"": {
            ""command"": ""START_SECONDS_TO_OVERRIDE"",
            ""seconds"": " + secondsToOpen + @"
        }
    }
}";

                await Location.GardenaClient.SendControlAsync(GardenaId, json, cancellationToken);
                Activity = "OPENING";
            }
        }

        [Operation]
        public async Task CloseValveAsync(CancellationToken cancellationToken)
        {
            if (Location?.GardenaClient != null && GardenaId != null)
            {
                var json = @"{
    ""data"": {
        ""type"": ""VALVE_CONTROL"",
        ""id"": """ + Guid.NewGuid().ToString() + @""",
        ""attributes"": {
            ""command"": ""STOP_UNTIL_NEXT_TASK""
        }
    }
}";

                await Location.GardenaClient.SendControlAsync(GardenaId, json, cancellationToken);
                Activity = "CLOSING";
            }
        }

        internal override GardenaValve Update(JObject data)
        {
            GardenaId = data?["id"]?.Value<string>();

            var array = GardenaId?.Split(':');
            if (array?.Length == 2)
            {
                ValveIndex = int.Parse(array[1]);
            }

            Title = data?["attributes"]?["name"]?["value"]?.Value<string>() ?? Title;
            Activity = data?["attributes"]?["activity"]?["value"]?.Value<string>() ?? Activity;
            State = data?["attributes"]?["state"]?["value"]?.Value<string>() ?? State;
            LastErrorCode = data?["attributes"]?["lastErrorCode"]?["value"]?.Value<string>() ?? LastErrorCode;

            LastUpdated = DateTimeOffset.Now; // TODO: Use last updated for every value
            return this;
        }

        internal override GardenaValve UpdateCommon(JObject data)
        {
            return this;
        }
    }
}

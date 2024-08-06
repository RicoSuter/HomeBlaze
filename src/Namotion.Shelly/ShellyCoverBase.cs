using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading;

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Presentation;

using Namotion.Proxy;

namespace Namotion.Shelly
{
    [GenerateProxy]
    public class ShellyCoverBase :
        IThing,
        IIconProvider,
        IPowerConsumptionSensor,
        IRollerShutter,
        ILastUpdatedProvider
    {
        string IThing.Id => Parent!.Id + "/cover";

        string? IThing.Title => "Cover";

        string IIconProvider.IconName => "fas fa-bars";

        [ParentThing]
        internal ShellyDevice? Parent { get; set; }

        public DateTimeOffset? LastUpdated => Parent?.LastUpdated;

        [State(Unit = StateUnit.Percent)]
        public virtual decimal? Position => (100 - CurrentPosition) / 100m;

        public virtual RollerShutterState State => LastState switch
        {
            "open" => RollerShutterState.Opening,
            "close" => RollerShutterState.Closing,
            "stop" =>
                ((IRollerShutter)this).IsFullyOpen == true ? RollerShutterState.Open :
                ((IRollerShutter)this).IsFullyClosed == true ? RollerShutterState.Closed :
                RollerShutterState.PartiallyOpen,

            _ => IsCalibrating == true ? RollerShutterState.Calibrating :
                RollerShutterState.Unknown
        };

        [State]
        public virtual bool? IsMoving => PowerConsumption > 1;

        [State(Unit = StateUnit.Watt)]
        [JsonPropertyName("power")]
        public virtual decimal? PowerConsumption { get; set; }

        [JsonPropertyName("state"), State]
        public virtual string? LastState { get; set; }

        [JsonPropertyName("source"), State]
        public virtual string? Source { get; set; }

        [JsonPropertyName("is_valid"), State]
        public virtual bool? IsValid { get; set; }

        [JsonPropertyName("safety_switch"), State]
        public virtual bool? IsSafetySwitchTriggered { get; set; }

        [JsonPropertyName("overtemperature"), State]
        public virtual bool? OvertemperatureOccurred { get; set; }

        [JsonPropertyName("stop_reason"), State]
        public virtual string? StopReason { get; set; }

        [JsonPropertyName("last_direction"), State]
        public virtual string? LastDirection { get; set; }

        [JsonPropertyName("current_pos")]
        public virtual int? CurrentPosition { get; set; }

        [JsonPropertyName("calibrating"), State]
        public virtual bool? IsCalibrating { get; set; }

        [JsonPropertyName("positioning"), State]
        public virtual bool? IsPositioning { get; set; }

        [Operation]
        public async Task OpenAsync(CancellationToken cancellationToken)
        {
            // https://shelly-api-docs.shelly.cloud/gen2/ComponentsAndServices/Cover#http-endpoint-rollerid

            await Parent!.CallHttpGetAsync("roller/0?go=open", cancellationToken);
        }

        [Operation]
        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            await Parent!.CallHttpGetAsync("roller/0?go=close", cancellationToken);
        }

        [Operation]
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Parent!.CallHttpGetAsync("roller/0?go=stop", cancellationToken);
        }
    }
}

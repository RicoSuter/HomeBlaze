using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading;

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Presentation;

using Namotion.Interceptor.Attributes;

namespace Namotion.Shelly
{
    [InterceptorSubject]
    public partial class ShellyCover :
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

        [Derived]
        [State(Unit = StateUnit.Percent)]
        public decimal? Position => (100 - CurrentPosition) / 100m;

        [Derived]
        public RollerShutterState State => LastState switch
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
        [Derived]
        public  bool? IsMoving => PowerConsumption > 1;

        [State(Unit = StateUnit.Watt)]
        [JsonPropertyName("power")]
        public partial decimal? PowerConsumption { get; set; }

        [JsonPropertyName("state"), State]
        public partial string? LastState { get; set; }

        [JsonPropertyName("source"), State]
        public partial string? Source { get; set; }

        [JsonPropertyName("is_valid"), State]
        public partial bool? IsValid { get; set; }

        [JsonPropertyName("safety_switch"), State]
        public partial bool? IsSafetySwitchTriggered { get; set; }

        [JsonPropertyName("overtemperature"), State]
        public partial bool? OvertemperatureOccurred { get; set; }

        [JsonPropertyName("stop_reason"), State]
        public partial string? StopReason { get; set; }

        [JsonPropertyName("last_direction"), State]
        public partial string? LastDirection { get; set; }

        [JsonPropertyName("current_pos")]
        public partial int? CurrentPosition { get; set; }

        [JsonPropertyName("calibrating"), State]
        public partial bool? IsCalibrating { get; set; }

        [JsonPropertyName("positioning"), State]
        public partial bool? IsPositioning { get; set; }

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

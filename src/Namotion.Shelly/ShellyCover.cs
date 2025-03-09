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

namespace Namotion.Shelly;

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

    [State]
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
    [JsonPropertyName("power"), JsonInclude]
    public partial decimal? PowerConsumption { get; internal set; }

    [State]
    [JsonPropertyName("state"), JsonInclude]
    public partial string? LastState { get; internal set; }

    [State]
    [JsonPropertyName("source"), JsonInclude]
    public partial string? Source { get; internal set; }

    [State]
    [JsonPropertyName("is_valid"), JsonInclude]
    public partial bool? IsValid { get; internal set; }

    [State]
    [JsonPropertyName("safety_switch"), JsonInclude]
    public partial bool? IsSafetySwitchTriggered { get; internal set; }

    [State]
    [JsonPropertyName("overtemperature"), JsonInclude]
    public partial bool? OvertemperatureOccurred { get; internal set; }

    [State]
    [JsonPropertyName("stop_reason"), JsonInclude]
    public partial string? StopReason { get; internal set; }

    [State]
    [JsonPropertyName("last_direction"), JsonInclude]
    public partial string? LastDirection { get; internal set; }

    [JsonPropertyName("current_pos"), JsonInclude]
    public partial int? CurrentPosition { get; internal set; }

    [State]
    [JsonPropertyName("calibrating"), JsonInclude]
    public partial bool? IsCalibrating { get; internal set; }

    [State]
    [JsonPropertyName("positioning"), JsonInclude]
    public partial bool? IsPositioning { get; internal set; }

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
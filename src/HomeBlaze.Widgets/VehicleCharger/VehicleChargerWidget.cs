using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions;
using System.ComponentModel;
using System;

using HomeBlaze.Abstractions.Services;
using HomeBlaze.Abstractions.Devices.Energy;

namespace HomeBlaze.Widgets.VehicleCharger;

[DisplayName("Vehicle Charger")]
[ThingWidget(typeof(VehicleChargerWidgetComponent))]
public class VehicleChargerWidget : IThing
{
    private readonly IThingManager _thingManager;

    [Configuration(IsIdentifier = true)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string? Title => (VehicleCharger as IThing)?.Title;

    [Configuration]
    public string? VehicleChargerId { get; set; }

    public IVehicleCharger? VehicleCharger => _thingManager.TryGetById(VehicleChargerId) as IVehicleCharger;

    [Configuration]
    public decimal Scale { get; set; } = 1.0m;

    public VehicleChargerWidget(IThingManager thingManager)
    {
        _thingManager = thingManager;
    }
}

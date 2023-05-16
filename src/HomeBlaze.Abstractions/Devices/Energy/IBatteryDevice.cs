using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;

namespace HomeBlaze.Abstractions.Devices.Energy
{
    public interface IBatteryDevice : IThing, IIconProvider
    {
        [State(Unit = StateUnit.Percent)]
        decimal? BatteryLevel { get; }

        string IIconProvider.IconName =>
           BatteryLevel < 0.1m ? "fas fa-thermometer-empty" :
           BatteryLevel < 0.25m ? "fas fa-battery-quarter" :
           BatteryLevel < 0.75m ? "fas fa-battery-half" :
           BatteryLevel < 1 ? "fas fa-battery-three-quarters" :
           "fas fa-battery-full";
    }
}

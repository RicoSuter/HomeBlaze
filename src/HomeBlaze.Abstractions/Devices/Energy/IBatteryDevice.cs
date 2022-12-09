using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices.Energy
{
    public interface IBatteryDevice : IThing
    {
        [State(Unit = StateUnit.Percent)]
        decimal? BatteryLevel { get; }
    }
}

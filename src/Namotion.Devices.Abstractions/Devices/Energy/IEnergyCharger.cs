using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices.Energy
{
    public interface IEnergyCharger
    {
        [State]
        bool? IsPluggedIn { get; }

        [State]
        bool? IsCharging { get; }
    }
}

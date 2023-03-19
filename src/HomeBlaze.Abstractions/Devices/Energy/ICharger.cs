using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices.Energy
{
    public interface ICharger : IThing
    {
        [State]
        bool? IsPluggedIn { get; }

        [State]
        bool? IsCharging { get; }
    }
}

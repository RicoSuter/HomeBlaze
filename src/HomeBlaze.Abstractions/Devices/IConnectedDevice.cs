using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices
{
    public interface IConnectedDevice : IThing
    {
        [State]
        bool IsConnected { get; }
    }
}

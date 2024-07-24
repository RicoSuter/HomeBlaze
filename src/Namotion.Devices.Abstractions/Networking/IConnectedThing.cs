using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Networking
{
    public interface IConnectedThing
    {
        [State]
        bool IsConnected { get; }
    }
}

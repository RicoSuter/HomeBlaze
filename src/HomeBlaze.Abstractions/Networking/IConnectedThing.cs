using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Networking
{
    public interface IConnectedThing : IThing
    {
        [State]
        bool IsConnected { get; }
    }
}

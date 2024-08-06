using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Networking
{
    public interface IServerThing
    {
        [State]
        bool IsListening { get; }

        [State]
        int? NumberOfClients { get; }
    }
}

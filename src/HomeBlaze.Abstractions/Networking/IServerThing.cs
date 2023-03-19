using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Networking
{
    public interface IServerThing : IThing
    {
        [State]
        bool IsListening { get; }

        [State]
        int? NumberOfClients { get; }
    }
}

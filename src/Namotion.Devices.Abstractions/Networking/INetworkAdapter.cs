using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Networking
{
    public interface INetworkAdapter : IConnectedThing
    {
        [State]
        public string? IpAddress => null;

        [State]
        public string? Host => null;

        [State]
        public string? MacAddress => null;

        [State]
        public int[]? Ports => null;
    }
}

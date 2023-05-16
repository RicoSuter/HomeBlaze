namespace HomeBlaze.Abstractions.Devices
{
    public interface IHubDevice
    {
        public IEnumerable<IThing> Devices { get; }
    }
}

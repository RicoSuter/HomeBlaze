using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices.Light
{
    public interface ILightbulb : ISwitchDevice
    {
        [State(Unit = StateUnit.Lumen)]
        public decimal? Lumen { get; }
    }
}

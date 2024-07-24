using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface IPresenceSensor
    {
        [State]
        public bool? IsPresent { get; }
    }
}

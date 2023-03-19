using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface IPresenceSensor : IThing
    {
        [State]
        public bool? IsPresent { get; }
    }
}

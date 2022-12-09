using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface ITemperatureSensor : IThing
    {
        [State(Unit = StateUnit.DegreeCelsius)]
        public decimal? Temperature { get; }
    }
}

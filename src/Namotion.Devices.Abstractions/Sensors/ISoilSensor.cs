using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface ISoilSensor
    {
        [State(Unit = StateUnit.Percent)]
        public decimal? SoilHumidity { get; }

        [State(Unit = StateUnit.DegreeCelsius)]
        public decimal? SoilTemperature { get; }
    }
}
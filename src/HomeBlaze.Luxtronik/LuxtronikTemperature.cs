using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Sensors;

namespace HomeBlaze.Luxtronik
{
    public class LuxtronikTemperature : ITemperatureSensor
    {
        public string? Id { get; internal set; }

        public string? Title { get; internal set; }

        [State(Unit = StateUnit.DegreeCelsius)]
        public decimal? Temperature { get; internal set; }
    }
}

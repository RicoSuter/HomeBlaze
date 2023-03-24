using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Sensors;

namespace HomeBlaze.Luxtronik
{
    public class LuxtronikTemperature : ITemperatureSensor
    {
        private LuxtronikDevice _device;
        private string _property;

        public LuxtronikTemperature(LuxtronikDevice device, string property)
        {
            _device = device;
            _property = property;
        }

        public string? Id => _device + "/" + _property;

        public string? Title { get; internal set; }

        [State(Unit = StateUnit.DegreeCelsius)]
        public decimal? Temperature { get; internal set; }
    }
}

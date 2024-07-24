using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface ITemperatureSensor : IIconProvider
    {
        [State(Unit = StateUnit.DegreeCelsius)]
        public decimal? Temperature { get; }

        string IIconProvider.IconName =>
            Temperature < 0 ? "fas fa-thermometer-empty" :
            Temperature < 10 ? "fas fa-fas fa-thermometer-quarter" :
            Temperature < 20 ? "fas fa-thermometer-half" :
            Temperature < 30 ? "fas fa-thermometer-three-quarters" :
            "fas fa-thermometer-full";
    }
}

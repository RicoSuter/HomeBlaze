using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface ILightSensor : IThing
    {
        /// <summary>
        /// Gets the light level in Lux.
        /// </summary>
        [State(Unit = StateUnit.Lux)]
        public decimal? LightLevel { get; }
    }
}

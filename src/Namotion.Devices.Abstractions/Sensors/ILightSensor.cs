using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface ILightSensor : IIconProvider
    {
        /// <summary>
        /// Gets the light level in Lux.
        /// </summary>
        [State(Unit = StateUnit.Lux)]
        public decimal? LightLevel { get; }

        string IIconProvider.IconName => "fas fa-sun";
    }
}

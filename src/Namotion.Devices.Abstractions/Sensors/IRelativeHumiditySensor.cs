using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface IRelativeHumiditySensor : IIconProvider
    {
        /// <summary>
        /// Gets the relative humidity in percent (0..1).
        /// </summary>
        [State(Unit = StateUnit.Percent)]
        public decimal? RelativeHumidity { get; }

        string IIconProvider.IconName => "fa-solid fa-droplet";
    }
}

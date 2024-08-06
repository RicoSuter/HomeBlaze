using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface IRainSensor : IIconProvider
    {
        /// <summary>
        /// Gets the current rain rate.
        /// </summary>
        [State(Unit = StateUnit.MillimeterPerHour)]
        public decimal? RainRate { get; }

        /// <summary>
        /// Gets a value indicating whether the sensor currently reports rain.
        /// </summary>
        [State]
        public bool? IsRaining => RainRate.HasValue ? RainRate > 0 : null;

        /// <summary>
        /// Gets the cumulative measured rain.
        /// </summary>
        [State(Unit = StateUnit.Millimeter, IsCumulative = true)]
        public decimal? TotalRain { get; }

        string IIconProvider.IconName => "fa-solid fa-cloud-rain";
    }
}

using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices
{
    public interface IRollerShutter
    {
        /// <summary>
        /// Gets the position of the roller shutter where 0 is fully open (window not covered) and 1 is fully closed (window fully covered).
        /// </summary>
        [State(Unit = StateUnit.Percent)]
        decimal? Position { get; }

        [State]
        bool? IsMoving { get; }

        [State]
        bool? IsCalibrating { get; }

        /// <summary>
        /// Specifies whether the roller shutter is fully open (window is not covered at all).
        /// </summary>
        [State]
        public bool? IsFullyOpen => Position == 0m;

        /// <summary>
        /// Specifies whether the roller shutter is fully closed (window is fully covered).
        /// </summary>
        [State]
        public bool? IsFullyClosed => Position == 1m;

        Task CloseAsync(CancellationToken cancellationToken);

        Task OpenAsync(CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);
    }
}
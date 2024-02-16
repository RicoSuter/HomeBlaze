using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices
{
    public interface IRollerShutter
    {
        [State(Unit = StateUnit.Percent)]
        decimal? Position { get; }

        [State]
        bool? IsMoving { get; }

        [State]
        bool? IsCalibrating { get; }

        [State]
        public bool? IsFullyOpen => Position == 1m;

        [State]
        public bool? IsFullyClosed => Position == 0m;

        Task CloseAsync(CancellationToken cancellationToken);

        Task OpenAsync(CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);
    }
}
using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices
{
    public interface IRollerShutter
    {
        [State(Unit = StateUnit.Percent)]
        decimal? Position { get; }

        [State]
        public bool? IsFullyOpen => Position == 100;

        [State]
        public bool? IsFullyClosed => Position == 0;

        [State]
        bool? IsCalibrating { get; }

        Task CloseAsync(CancellationToken cancellationToken);

        Task OpenAsync(CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);
    }
}
using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices.Energy
{
    public interface IPowerRelay : IThing
    {
        [State]
        public bool? IsPowerOn { get; }

        [Operation]
        Task TurnPowerOnAsync(CancellationToken cancellationToken);

        [Operation]
        Task TurnPowerOffAsync(CancellationToken cancellationToken);

        [Operation]
        async Task TogglePowerAsync(CancellationToken cancellationToken)
        {
            if (IsPowerOn == false)
            {
                await TurnPowerOnAsync(cancellationToken);
            }
            else if (IsPowerOn == true)
            {
                await TurnPowerOffAsync(cancellationToken);
            }
        }
    }
}

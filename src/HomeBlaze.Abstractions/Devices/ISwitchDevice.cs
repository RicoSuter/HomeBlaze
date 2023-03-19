using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices
{
    public interface ISwitchDevice : IThing
    {
        [State]
        public bool? IsOn { get; }

        [Operation]
        Task TurnOnAsync(CancellationToken cancellationToken = default);

        [Operation]
        Task TurnOffAsync(CancellationToken cancellationToken = default);

        [Operation]
        async Task ToggleAsync(CancellationToken cancellationToken = default)
        {
            if (IsOn == false)
            {
                await TurnOnAsync(cancellationToken);
            }
            else if (IsOn == true)
            {
                await TurnOffAsync(cancellationToken);
            }
        }
    }
}
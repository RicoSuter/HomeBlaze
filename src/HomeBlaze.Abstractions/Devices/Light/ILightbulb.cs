using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices.Light
{
    public interface ILightbulb : IThing
    {
        [State]
        public bool? IsLightOn { get; }

        [State(Unit = StateUnit.Lumen)]
        public decimal? Lumen { get; }

        [Operation]
        Task TurnLightOnAsync(CancellationToken cancellationToken);

        [Operation]
        Task TurnLightOffAsync(CancellationToken cancellationToken);

        [Operation]
        async Task ToggleLightAsync(CancellationToken cancellationToken)
        {
            if (IsLightOn == true)
            {
                await TurnLightOffAsync(cancellationToken);
            }
            else if (IsLightOn == false)
            {
                await TurnLightOnAsync(cancellationToken);
            }
        }
    }
}

using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices.Light
{
    public interface IDimmerLightbulb : ILightbulb
    {
        [State(Unit = StateUnit.Percent)]
        decimal? Brightness { get; }

        [Operation]
        Task DimmAsync(decimal brightness, CancellationToken cancellationToken);
    }
}

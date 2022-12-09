using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices.Light
{
    public interface IColorTemperatureLightbulb : ILightbulb
    {
        [State]
        decimal? ColorTemperature { get; }

        [Operation]
        Task ChangeTemperatureAsync(decimal colorTemperature, CancellationToken cancellationToken);
    }
}

using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices.Light
{
    public interface IColorTemperatureLightbulb : ILightbulb
    {
        [State(Unit = StateUnit.Percent)]
        decimal? ColorTemperature { get; }

        [Operation]
        Task ChangeTemperatureAsync([OperationParameter(Unit = StateUnit.Percent)] decimal colorTemperature, CancellationToken cancellationToken = default);
    }
}

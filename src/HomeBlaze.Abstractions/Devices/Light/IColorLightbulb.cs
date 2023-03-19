using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices.Light
{
    public interface IColorLightbulb : ILightbulb
    {
        [State(Unit = StateUnit.HexColor)]
        string? Color { get; }

        [Operation]
        Task ChangeColorAsync(
            [OperationParameter(Unit = StateUnit.HexColor)] string color, 
            CancellationToken cancellationToken = default);
    }
}

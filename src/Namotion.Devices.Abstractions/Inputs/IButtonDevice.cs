using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Inputs
{
    public interface IButtonDevice
    {
        [State]
        ButtonState? ButtonState { get; }
    }
}
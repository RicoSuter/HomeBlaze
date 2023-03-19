using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Inputs
{
    public interface IButtonDevice : IThing
    {
        [State]
        ButtonState? ButtonState { get; }
    }
}
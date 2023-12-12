using HomeBlaze.Abstractions.Messages;

namespace HomeBlaze.Abstractions.Inputs
{
    public class ButtonEvent : IEvent
    {
        public required string ThingId { get; init; }

        public required ButtonState ButtonState { get; init; }
    }
}

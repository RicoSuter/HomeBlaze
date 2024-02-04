using HomeBlaze.Abstractions.Messages;

namespace HomeBlaze.Abstractions.Inputs
{
    public class ButtonEvent : IThingEvent
    {
        public required string ThingId { get; init; }

        public required ButtonState ButtonState { get; init; }
    }
}

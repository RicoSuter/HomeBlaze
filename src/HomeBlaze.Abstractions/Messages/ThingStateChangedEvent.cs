using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Messages;

namespace HomeBlaze.Messages
{
    public record ThingStateChangedEvent : IEvent
    {
        public DateTimeOffset ChangeDate { get; init; }

        public string? PropertyName { get; init; }

        public object? OldValue { get; init; }

        public object? NewValue { get; init; }

        public IThing Thing { get; }

        public ThingStateChangedEvent(IThing thing)
        {
            Thing = thing;
        }
    }
}

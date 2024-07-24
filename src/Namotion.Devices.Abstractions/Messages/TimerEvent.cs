using HomeBlaze.Abstractions.Messages;

namespace HomeBlaze.Messages
{
    public record TimerEvent : IEvent
    {
        public TimeSpan TimeOfDay => DateTime.TimeOfDay;

        public DateTimeOffset DateTime { get; init; }
    }
}
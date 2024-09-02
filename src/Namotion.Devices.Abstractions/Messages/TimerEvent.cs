using Namotion.Devices.Abstractions.Messages;

namespace HomeBlaze.Messages
{
    public record TimerEvent : IEvent
    {
        public required object Source { get; init; }

        public TimeSpan TimeOfDay => DateTime.TimeOfDay;

        public DateTimeOffset DateTime { get; init; }
    }
}
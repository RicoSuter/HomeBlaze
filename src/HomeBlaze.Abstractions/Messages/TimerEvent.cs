namespace HomeBlaze.Abstractions.Messages
{
    public record TimerEvent : IEvent
    {
        public TimeSpan TimeOfDay => DateTime.TimeOfDay;

        public DateTimeOffset DateTime { get; init; }
    }
}
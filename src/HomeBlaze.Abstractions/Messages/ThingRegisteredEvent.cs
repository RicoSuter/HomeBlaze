namespace HomeBlaze.Abstractions.Messages
{
    public record ThingRegisteredEvent : IEvent
    {
        public IThing Thing { get; }

        public ThingRegisteredEvent(IThing thing)
        {
            Thing = thing;
        }
    }
}

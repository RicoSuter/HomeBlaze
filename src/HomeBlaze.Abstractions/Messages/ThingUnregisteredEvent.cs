namespace HomeBlaze.Abstractions.Messages
{
    public record ThingUnregisteredEvent : IEvent
    {
        public IThing Thing { get; }

        public ThingUnregisteredEvent(IThing thing)
        {
            Thing = thing;
        }
    }
}

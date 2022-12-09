namespace HomeBlaze.Abstractions.Messages
{
    public record RootThingLoadedEvent : IEvent
    {
        public IThing Thing { get; }

        public RootThingLoadedEvent(IThing thing)
        {
            Thing = thing;
        }
    }
}

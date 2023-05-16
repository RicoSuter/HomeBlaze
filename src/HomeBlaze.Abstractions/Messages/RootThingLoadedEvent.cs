using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Messages;

namespace HomeBlaze.Messages
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

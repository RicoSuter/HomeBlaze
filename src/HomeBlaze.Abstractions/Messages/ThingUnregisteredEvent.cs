using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Messages;

namespace HomeBlaze.Messages
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

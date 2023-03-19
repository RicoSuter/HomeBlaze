using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Messages;

namespace HomeBlaze.Messages
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

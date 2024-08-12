using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Messages;

namespace HomeBlaze.Messages
{
    public record DetectChangesEvent : IEvent
    {
        public IThing Thing { get; }

        public DetectChangesEvent(IThing thing)
        {
            Thing = thing;
        }
    }
}

using Namotion.Devices.Abstractions.Messages;

using HomeBlaze.Abstractions;

namespace HomeBlaze.Messages
{
    public record DetectChangesEvent : IEvent
    {
        public object Source { get; }

        public DetectChangesEvent(IThing thing)
        {
            Source = thing;
        }
    }
}

using HomeBlaze.Abstractions;
using Namotion.Devices.Abstractions.Messages;

namespace HomeBlaze.Messages
{
    public record ThingUnregisteredEvent : IEvent
    {
        public object Source { get; }

        public ThingUnregisteredEvent(IThing thing)
        {
            Source = thing;
        }
    }
}

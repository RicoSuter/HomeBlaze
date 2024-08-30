using HomeBlaze.Abstractions;
using Namotion.Devices.Abstractions.Messages;

namespace HomeBlaze.Messages
{
    public record ThingRegisteredEvent : IEvent
    {
        public object Source { get; }

        public ThingRegisteredEvent(IThing thing)
        {
            Source = thing;
        }
    }
}

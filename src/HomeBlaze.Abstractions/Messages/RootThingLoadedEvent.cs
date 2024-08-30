using HomeBlaze.Abstractions;
using Namotion.Devices.Abstractions.Messages;

namespace HomeBlaze.Messages
{
    public record RootThingLoadedEvent : IEvent
    {
        public object Source { get; }

        public RootThingLoadedEvent(IThing thing)
        {
            Source = thing;
        }
    }
}

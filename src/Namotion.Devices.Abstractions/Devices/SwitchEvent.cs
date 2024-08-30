using Namotion.Devices.Abstractions.Messages;

namespace HomeBlaze.Abstractions.Inputs
{
    public class SwitchEvent : IEvent
    {
        public required object Source { get; init; }

        public required bool IsOn { get; init; }
    }
}

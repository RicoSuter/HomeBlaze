using HomeBlaze.Abstractions.Inputs;
using Namotion.Devices.Abstractions.Messages;

namespace Namotion.Devices.Abstractions.Inputs
{
    public class ButtonEvent : IEvent
    {
        public required object Source { get; init; }

        public required ButtonState ButtonState { get; init; }
    }
}

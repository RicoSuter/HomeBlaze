using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
//using HomeBlaze.Abstractions.Presentation;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface IDoorSensor : IIconProvider
    {
        [State]
        bool? IsDoorClosed => DoorState.HasValue ? DoorState == Sensors.DoorState.Closed : null;

        [State]
        DoorState? DoorState { get; }

        string IIconProvider.IconName =>
            IsDoorClosed == true ? "fas fa-door-closed" :
            "fas fa-door-open";
    }

    /// <summary>
    /// Represents the various states of a door.
    /// </summary>
    public enum DoorState
    {
        /// <summary>
        /// The door is fully open.
        /// </summary>
        Open,

        /// <summary>
        /// The door is in the process of opening.
        /// </summary>
        Opening,

        /// <summary>
        /// The door is partially open but not fully closed.
        /// </summary>
        PartiallyOpen,

        /// <summary>
        /// The door is in the process of closing.
        /// </summary>
        Closing,

        /// <summary>
        /// The door is fully closed.
        /// </summary>
        Closed,

        /// <summary>
        /// The state of the door is unknown.
        /// </summary>
        Unknown
    }
}
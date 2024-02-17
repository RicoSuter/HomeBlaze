using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface IDoorSensor : IThing, IIconProvider
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
        /// Indicates that the door is fully open.
        /// </summary>
        Open,

        /// <summary>
        /// Indicates that the door is in the process of opening.
        /// </summary>
        Opening,

        /// <summary>
        /// Indicates that the door is fully closed.
        /// </summary>
        Closed,

        /// <summary>
        /// Indicates that the door is in the process of closing.
        /// </summary>
        Closing,

        /// <summary>
        /// Indicates that the door is in an intermediate position, neither fully open nor fully closed.
        /// </summary>
        PartiallyOpen,

        /// <summary>
        /// Indicates an unknown or undetermined state of the door.
        /// </summary>
        Unknown
    }

}
using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface IDoorSensor : IThing
    {
        [State]
        bool? IsDoorClosed => DoorState.HasValue ? DoorState == Sensors.DoorState.Close : null;

        [State]
        DoorState? DoorState { get; }
    }

    public enum DoorState
    {
        Open,
        Opening,
        Close,
        Closing
    }
}
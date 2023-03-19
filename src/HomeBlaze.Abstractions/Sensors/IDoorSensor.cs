using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface IDoorSensor : IThing, IIconProvider
    {
        [State]
        bool? IsDoorClosed => DoorState.HasValue ? DoorState == Sensors.DoorState.Close : null;

        [State]
        DoorState? DoorState { get; }

        string IIconProvider.IconName =>
            IsDoorClosed == true ? "fas fa-door-closed" : 
            "fas fa-door-open";
    }

    public enum DoorState
    {
        Open,
        Opening,
        Close,
        Closing
    }
}
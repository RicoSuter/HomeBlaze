using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Sensors;
using ZWave.CommandClasses;

namespace HomeBlaze.Zwave.Components
{
    public class ZwaveDoorSensorComponent : ZwaveClassComponent, IDoorSensor
    {
        protected override string Class => "DoorSensor";

        // see https://github.com/zwave-js/node-zwave-js/blob/master/packages/config/config/notifications.json#L406

        public DoorState? DoorState =>
            ParentNotification.Event == NotificationState.WindowDoorOpen ? Abstractions.Sensors.DoorState.Open :
            ParentNotification.Event == NotificationState.WindowDoorClosed ? Abstractions.Sensors.DoorState.Closed :
            null;

        public ZwaveNotificationComponent ParentNotification { get; }

        [State]
        public byte EndPointId { get; }

        public ZwaveDoorSensorComponent(ZwaveNotificationComponent parent, byte endPointId)
            : base(parent.ParentDevice)
        {
            ParentNotification = parent;
            EndPointId = endPointId;
        }
    }
}

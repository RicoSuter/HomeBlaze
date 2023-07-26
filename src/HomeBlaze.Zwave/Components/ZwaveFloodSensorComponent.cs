using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Zwave.Components
{
    public class ZwaveFloodSensorComponent : ZwaveClassComponent
    {
        protected override string Class => "FloodSensor";

        [State]
        public bool? IsFlooding { get; set; }

        [State]
        public byte EndPointId { get; }

        public ZwaveNotificationComponent ParentNotification { get; }

        public ZwaveFloodSensorComponent(ZwaveNotificationComponent parent, byte endPointId)
            : base(parent.ParentDevice)
        {
            ParentNotification = parent;
            EndPointId = endPointId;
        }
    }
}

using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Zwave.Components
{
    public class ZwaveFloodSensorComponent : ZwaveClassComponent
    {
        protected override string Class => "FloodSensor";

        [State]
        public bool? IsFlooding { get; set; }

        public ZwaveNotificationComponent ParentNotification { get; }

        public ZwaveFloodSensorComponent(ZwaveNotificationComponent parent)
            : base(parent.ParentDevice)
        {
            ParentNotification = parent;
        }
    }
}

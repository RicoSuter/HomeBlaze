using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Zwave
{
    public class ZwaveNotificationComponent : ZwaveClassComponent
    {
        protected override string Class => "Notification";

        [State]
        public int Level { get; set; }

        [State]
        public int Detail { get; set; }

        public ZwaveNotificationComponent(ZwaveDevice parent)
            : base(parent)
        {
        }
    }
}

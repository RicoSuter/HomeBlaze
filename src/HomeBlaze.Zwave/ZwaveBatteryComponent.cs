using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Presentation;

namespace HomeBlaze.Zwave
{
    public class ZwaveBatteryComponent : ZwaveClassComponent, IIconProvider, IBatteryDevice
    {
        public decimal? BatteryLevel { get; internal set; }

        protected override string Class => "Battery";

        public string IconName => "fas fa-battery-three-quarters";

        public ZwaveBatteryComponent(ZwaveDevice parent)
            : base(parent)
        {
        }
    }
}

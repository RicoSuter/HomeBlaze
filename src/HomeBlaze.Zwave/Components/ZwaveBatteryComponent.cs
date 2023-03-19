using HomeBlaze.Abstractions.Devices.Energy;

namespace HomeBlaze.Zwave.Components
{
    public class ZwaveBatteryComponent : ZwaveClassComponent, IBatteryDevice
    {
        public decimal? BatteryLevel { get; internal set; }

        protected override string Class => "Battery";

        public ZwaveBatteryComponent(ZwaveDevice parent)
            : base(parent)
        {
        }
    }
}

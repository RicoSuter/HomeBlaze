using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Energy;
using Q42.HueApi.Models;
using System;
using System.Linq;

namespace HomeBlaze.Philips.Hue
{
    public class HueDimmerSwitchDevice : HueSwitchDevice, IBatteryDevice
    {
        [State]
        public decimal? BatteryLevel => Sensor?.Config.Battery / 100m;

        public HueDimmerSwitchDevice(Sensor sensor, HueBridge bridge)
            : base(sensor, bridge)
        {
            Update(sensor);
        }

        internal HueDimmerSwitchDevice Update(Sensor sensor)
        {
            Sensor = sensor;
            Buttons = sensor
                .Capabilities
                .Inputs?
                .Select((s, i) => Buttons
                    .OfType<HueInputDeviceBase>()
                    .SingleOrDefault(d => d.ReferenceId == i)?.Update(s)
                        ?? new HueInputDeviceBase(i, "Button " + (i + 1), s, this))
                .ToArray() ?? Array.Empty<HueInputDeviceBase>();

            return this;
        }
    }
}

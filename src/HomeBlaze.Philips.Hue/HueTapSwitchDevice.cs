using HomeBlaze.Abstractions.Presentation;
using HueApi.Models;

namespace HomeBlaze.Philips.Hue
{
    public class HueTapSwitchDevice : HueSwitchDevice, IIconProvider
    {
        public HueTapSwitchDevice(Device sensor, HueBridge bridge)
            : base(sensor, bridge)
        {
            Update(sensor);
        }

        internal HueTapSwitchDevice Update(Device sensor)
        {
            Sensor = sensor;
            //Buttons = sensor
            //    .Capabilities
            //    .Inputs?
            //    .Select((s, i) => Buttons
            //        .OfType<HueInputDeviceBase>()
            //        .SingleOrDefault(d => d.ReferenceId == i)?.Update(s)
            //            ?? new HueInputDeviceBase(i, "Button " + (i + 1), s, this))
            //    .ToArray() ?? Array.Empty<HueInputDeviceBase>();

            return this;
        }
    }
}

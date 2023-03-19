using HomeBlaze.Abstractions.Presentation;
using Q42.HueApi.Models;
using System;
using System.Linq;

namespace HomeBlaze.Philips.Hue
{
    public class HueTapSwitchDevice : HueSwitchDevice, IIconProvider
    {
        public override string? Id => Bridge != null ?
            "hue.tap.switch." + Bridge.BridgeId + "." + Sensor.UniqueId :
            null;

        public HueTapSwitchDevice(Sensor sensor, HueBridge bridge)
            : base(sensor, bridge)
        {
            Update(sensor);
        }

        internal HueTapSwitchDevice Update(Sensor sensor)
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

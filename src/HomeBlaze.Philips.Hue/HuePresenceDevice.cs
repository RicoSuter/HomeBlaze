using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HueApi.Models;
using HueApi.Models.Sensors;

namespace HomeBlaze.Philips.Hue
{
    public class HuePresenceDevice :
        HueDevice,
        IThing,
        IIconProvider,
        IPresenceSensor,
        IBatteryDevice
    {
        private MotionResource _motion;

        public string IconName => "fas fa-running";

        [State]
        public bool? IsPresent => _motion.Motion.MotionState;

        [State]
        public decimal? BatteryLevel => null; // _sensor?.Config.Battery / 100m;

        public HuePresenceDevice(Device device, ZigbeeConnectivity? zigbeeConnectivity, MotionResource motion, HueBridge bridge)
            : base(device, zigbeeConnectivity, bridge)
        {
            _motion = motion;
            Update(device, zigbeeConnectivity, motion);
        }

        internal HuePresenceDevice Update(Device device, ZigbeeConnectivity? zigbeeConnectivity, MotionResource motion)
        {
            base.Update(device, zigbeeConnectivity);
            _motion = motion;
            return this;
        }
    }
}

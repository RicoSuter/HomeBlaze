using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HueApi.Models;
using HueApi.Models.Sensors;

namespace HomeBlaze.Philips.Hue
{
    public class HueMotionDevice :
        HueDevice,
        IThing,
        IIconProvider,
        IPresenceSensor,
        IBatteryDevice,
        ILightSensor,
        ITemperatureSensor
    {
        public override string IconName => "fas fa-running";

        internal MotionResource MotionResource { get; set; }

        internal DevicePower? DevicePowerResource { get; set; }

        internal TemperatureResource? TemperatureResource { get; set; }

        internal LightLevel? LightLevelResource { get; set; }

        [State]
        public bool? IsPresent => MotionResource.Motion.MotionReport?.Motion;

        [State]
        public decimal? BatteryLevel => DevicePowerResource?.PowerState?.BatteryLevel / 100m;

        [State]
        public decimal? Temperature => TemperatureResource?.Temperature.TemperatureValid == true ?
            TemperatureResource?.Temperature.TemperatureReport?.Temperature : null;

        [State]
        public decimal? LightLevel => LightLevelResource?.Enabled == true ? (decimal?)LightLevelResource?.Light.LightLevelReport.LuxLevel : null;

        public HueMotionDevice(Device device, ZigbeeConnectivity? zigbeeConnectivity, DevicePower? devicePower, TemperatureResource? temperature, LightLevel? lightLevel, MotionResource motion, HueBridge bridge)
            : base(device, zigbeeConnectivity, bridge)
        {
            DevicePowerResource = devicePower;
            TemperatureResource = temperature;
            LightLevelResource = lightLevel;
            MotionResource = motion;

            Update(device, zigbeeConnectivity, devicePower, temperature, lightLevel, motion);
        }

        internal HueMotionDevice Update(Device device, ZigbeeConnectivity? zigbeeConnectivity, DevicePower? devicePower, TemperatureResource? temperature, LightLevel? lightLevel, MotionResource motion)
        {
            Update(device, zigbeeConnectivity);

            DevicePowerResource = devicePower;
            TemperatureResource = temperature;
            LightLevelResource = lightLevel;
            MotionResource = motion;

            return this;
        }
    }
}

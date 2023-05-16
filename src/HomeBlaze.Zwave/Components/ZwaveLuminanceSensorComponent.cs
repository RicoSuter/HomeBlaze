using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Sensors;
using ZWave.CommandClasses;

namespace HomeBlaze.Zwave.Components
{
    public class ZwaveLuminanceSensorComponent : ZwaveSensorComponent, ILightSensor
    {
        // TODO(zwave): Check unit/scale and convert to lux if needed

        [State]
        public decimal? LightLevel => Value;

        public ZwaveLuminanceSensorComponent(ZwaveDevice parent)
            : base(parent, SensorType.Luminance)
        {
        }
    }
}

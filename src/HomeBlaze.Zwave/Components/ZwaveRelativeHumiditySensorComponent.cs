using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Sensors;
using ZWave.CommandClasses;

namespace HomeBlaze.Zwave.Components
{
    public class ZwaveRelativeHumiditySensorComponent : ZwaveSensorComponent, IRelativeHumiditySensor
    {
        [State]
        public decimal? RelativeHumidity => Value / 100m;

        public ZwaveRelativeHumiditySensorComponent(ZwaveDevice parent)
            : base(parent, SensorType.RelativeHumidity)
        {
        }
    }
}

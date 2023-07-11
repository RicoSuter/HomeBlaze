using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Sensors;
using System.Linq;
using ZWave.CommandClasses;

namespace HomeBlaze.Zwave.Components
{
    public class ZwaveRainSensorComponent : ZwaveSensorComponent, IRainSensor
    {
        private readonly ZwaveDevice _parent;

        [State]
        public decimal? RainRate => Value;

        [State]
        public decimal? TotalRain
        {
            get
            {
                // POPP Z-RAIN reports total rain via the general sensor

                var generalSensor = _parent.Things
                    .OfType<ZwaveSensorComponent>()
                    .FirstOrDefault(s => s.SensorType == SensorType.General);

                return generalSensor?.Value * (generalSensor?.Scale ?? 1);
            }
        }

        public ZwaveRainSensorComponent(ZwaveDevice parent)
            : base(parent, SensorType.RelativeHumidity)
        {
            _parent = parent;
        }
    }
}

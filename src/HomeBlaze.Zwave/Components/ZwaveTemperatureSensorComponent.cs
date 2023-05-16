using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using System;
using ZWave.CommandClasses;

namespace HomeBlaze.Zwave.Components
{
    public class ZwaveTemperatureSensorComponent : ZwaveSensorComponent, IIconProvider, ITemperatureSensor
    {
        [State]
        public decimal? Temperature => Value.HasValue ? Math.Round(
            Unit == "°F" ? (Value.Value - 32m) / 1.8m :
            Value.Value, 2) : null;

        public ZwaveTemperatureSensorComponent(ZwaveDevice parent)
            : base(parent, SensorType.Temperature)
        {
        }
    }
}

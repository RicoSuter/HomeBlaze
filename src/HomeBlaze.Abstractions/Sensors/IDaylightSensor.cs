using HomeBlaze.Abstractions.Attributes;
using System;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface IDaylightSensor : IThing
    {
        [State]
        public bool? IsDaylight { get; }
    }
}

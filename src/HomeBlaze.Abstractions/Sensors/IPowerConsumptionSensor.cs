﻿using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Sensors
{
    public interface IPowerConsumptionSensor : IThing
    {
        /// <summary>
        /// Gets the current power consumption in Watts/h.
        /// </summary>
        [State(Unit = StateUnit.Watt)]
        decimal? PowerConsumption { get; }
    }
}

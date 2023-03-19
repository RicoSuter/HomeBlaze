using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Sensors;
using ZWave.CommandClasses;

namespace HomeBlaze.Zwave.Components
{
    public class ZwavePowerConsumptionComponent : ZwaveClassComponent, IPowerConsumptionSensor
    {
        protected override string Class => "ElectricMeter";

        public decimal? PowerConsumption => Value;

        [State]
        public decimal? Value { get; internal set; }

        [State]
        public ElectricMeterScale Scale { get; internal set; }

        [State]
        public string? Unit { get; internal set; }

        public ZwavePowerConsumptionComponent(ZwaveDevice parent) : base(parent)
        {
        }
    }
}

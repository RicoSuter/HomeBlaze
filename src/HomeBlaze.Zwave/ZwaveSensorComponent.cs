using HomeBlaze.Abstractions.Attributes;
using ZWave.CommandClasses;

namespace HomeBlaze.Zwave
{
    public class ZwaveSensorComponent : ZwaveClassComponent
    {
        protected override string Class => "Sensor:" + SensorType.ToString();

        public override string Title => $"{SensorType} (Node {ParentDevice?.NodeId.ToString() ?? "n/a"})";

        [State]
        public decimal? Value { get; internal set; }

        [State]
        public byte? Scale { get; internal set; }

        [State]
        public string? Unit { get; internal set; }

        [State]
        public SensorType SensorType { get; internal set; }

        public ZwaveSensorComponent(ZwaveDevice parent, SensorType sensorType)
            : base(parent)
        {
            SensorType = sensorType;
        }
    }
}

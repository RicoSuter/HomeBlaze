using HomeBlaze.Abstractions.Attributes;
using System.Text.Json.Serialization;
using ZWave.CommandClasses;

namespace HomeBlaze.Zwave
{
    public class ZwaveSensorAlarmComponent : ZwaveClassComponent
    {
        protected override string Class => "SensorAlarm";

        public override string Title => $"SensorAlarm (Node {ParentDevice?.NodeId.ToString() ?? "n/a"})";

        [State]
        public byte? Level { get; internal set; }

        [State]
        public byte? Source { get; internal set; }

        [State]
        public AlarmType? AlarmType { get; internal set; }

        public ZwaveSensorAlarmComponent(ZwaveDevice parent)
            : base(parent)
        {
        }
    }
}

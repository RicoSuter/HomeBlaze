using HomeBlaze.Abstractions.Attributes;
using ZWave.CommandClasses;

namespace HomeBlaze.Zwave.Components
{
    public class ZwaveSensorAlarmComponent : ZwaveClassComponent
    {
        protected override string Class => "SensorAlarm";

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

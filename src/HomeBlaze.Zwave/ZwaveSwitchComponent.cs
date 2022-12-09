using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Energy;
using System.Threading;
using System.Threading.Tasks;
using ZWave.CommandClasses;

namespace HomeBlaze.Zwave
{
    public class ZwaveSwitchComponent : ZwaveClassComponent, IPowerRelay
    {
        private readonly SwitchBinary _switchBinary;

        protected override string Class => EndPointId + ":SwitchBinary";

        [State]
        public bool? IsPowerOn { get; private set; }

        [State]
        public byte EndPointId { get; }

        public ZwaveSwitchComponent(ZwaveDevice parent, SwitchBinary switchBinary, byte endPointId)
            : base(parent)
        {
            _switchBinary = switchBinary;
            EndPointId = endPointId;
        }

        internal void OnSwitchBinary(object? sender, ReportEventArgs<SwitchBinaryReport> e)
        {
            IsPowerOn = e.Report.Value;
            ParentDevice.Controller.ThingManager.DetectChanges(this);
        }

        public async Task TurnPowerOnAsync(CancellationToken cancellationToken)
        {
            await _switchBinary.Set(true, cancellationToken);

            IsPowerOn = true;
            ParentDevice.Controller.ThingManager.DetectChanges(this);
        }

        public async Task TurnPowerOffAsync(CancellationToken cancellationToken)
        {
            await _switchBinary.Set(false, cancellationToken);

            IsPowerOn = false;
            ParentDevice.Controller.ThingManager.DetectChanges(this);
        }
    }
}

using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Presentation;
using System.Threading;
using System.Threading.Tasks;
using ZWave.CommandClasses;

namespace HomeBlaze.Zwave.Components
{
    public class ZwaveSwitchComponent : ZwaveClassComponent, IIconProvider, IPowerRelay
    {
        private readonly SwitchBinary _switchBinary;

        protected override string Class => $"SwitchBinary:{EndPointId}";

        public string IconName => "fas fa-plug";

        [State]
        public bool? IsOn { get; private set; }

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
            IsOn = e.Report.CurrentValue;
            ParentDevice.Controller.DetectChanges(this);
        }

        public async Task TurnOnAsync(CancellationToken cancellationToken)
        {
            await _switchBinary.Set(true, cancellationToken);

            var report = await _switchBinary.Get(cancellationToken);
            IsOn = report.CurrentValue;

            ParentDevice.Controller.DetectChanges(this);
        }

        public async Task TurnOffAsync(CancellationToken cancellationToken)
        {
            await _switchBinary.Set(false, cancellationToken);

            var report = await _switchBinary.Get(cancellationToken);
            IsOn = report.CurrentValue;

            ParentDevice.Controller.DetectChanges(this);
        }
    }
}

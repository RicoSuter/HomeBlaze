using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Inputs;
using HomeBlaze.Abstractions.Presentation;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Logitech
{
    public class LogitechHarmonyDevice : IThing, IIconProvider, IActionDevice
    {
        private Harmony.Device _device;
        private readonly LogitechHarmonyHub _hub;

        public string Id => _hub.Id + "/devices/" + _device.ID;

        public string? Title => $"{_device.Label} ({_device.DeviceTypeDisplayName})" ?? "n/a";

        public string ReferenceId => _device.ID;

        public string IconName => "fab fa-hubspot";

        [State]
        public string Type => _device.Type;

        [State]
        public DateTimeOffset? LastUpdated { get; internal set; }

        [State]
        public IActionDevice.DeviceAction[] Actions { get; private set; } = Array.Empty<IActionDevice.DeviceAction>();

        public LogitechHarmonyDevice(Harmony.Device device, LogitechHarmonyHub hub)
        {
            _hub = hub;
            Update(device);
        }

        [MemberNotNull(nameof(_device))]
        internal LogitechHarmonyDevice Update(Harmony.Device device)
        {
            _device = device;

            LastUpdated = device != null ? DateTimeOffset.Now : null;
            Actions = _device.Functions
                .Select(a => new IActionDevice.DeviceAction
                {
                    Id = a.Name,
                    Title = a.Label
                })
                .ToArray();

            return this;
        }

        public async Task ExecuteActionAsync(string actionId, CancellationToken cancellationToken)
        {
            var function = _device.Functions.SingleOrDefault(f => f.Name == actionId);
            if (function != null && _hub?.Hub != null)
            {
                await _hub.Hub.PressButtonAsync(function);
            }
        }
    }
}

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HueApi.Models;
using MudBlazor;
using System;
using System.Linq;

namespace HomeBlaze.Philips.Hue
{
    public class HueButtonDevice :
        HueDevice,
        IThing,
        IIconProvider
    {
        public string IconName =>
            Buttons.Any(b => b.ButtonState != Abstractions.Inputs.ButtonState.None) ?
            Icons.Material.Filled.RadioButtonChecked :
            Icons.Material.Filled.RadioButtonUnchecked;

        [State]
        public HueButton[] Buttons { get; protected set; } = Array.Empty<HueButton>();

        public HueButtonDevice(Device device, ZigbeeConnectivity? zigbeeConnectivity, ButtonResource[] buttons, HueBridge bridge)
            : base(device, zigbeeConnectivity, bridge)
        {
            Update(device, zigbeeConnectivity, buttons);
        }

        internal HueButtonDevice Update(Device device, ZigbeeConnectivity? zigbeeConnectivity, ButtonResource[] buttons)
        {
            Update(device, zigbeeConnectivity);

            Buttons = buttons
                .Select((s, i) => Buttons
                    .OfType<HueButton>()
                    .SingleOrDefault(d => d.ReferenceId == s.Id)?.Update(s)
                        ?? new HueButton("Button " + (i + 1), s, this))
                .ToArray() ?? Array.Empty<HueButton>();

            return this;
        }
    }
}

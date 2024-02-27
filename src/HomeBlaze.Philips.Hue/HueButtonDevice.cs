using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Energy;
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
        IBatteryDevice,
        IIconProvider
    {
        public override string IconName =>
            Buttons.Any(b => b.ButtonState != Abstractions.Inputs.ButtonState.None) ?
            Icons.Material.Filled.RadioButtonChecked :
            Icons.Material.Filled.RadioButtonUnchecked;

        public override MudBlazor.Color IconColor => IsConnected ? MudBlazor.Color.Default : MudBlazor.Color.Error;

        internal DevicePower? DevicePowerResource { get; set; }

        [State]
        public decimal? BatteryLevel => DevicePowerResource?.PowerState?.BatteryLevel / 100m;

        [State]
        public HueButton[] Buttons { get; protected set; } = Array.Empty<HueButton>();

        public HueButtonDevice(Device device, ZigbeeConnectivity? zigbeeConnectivity, DevicePower? devicePower, ButtonResource[] buttons, HueBridge bridge)
            : base(device, zigbeeConnectivity, bridge)
        {
            Update(device, zigbeeConnectivity, devicePower, buttons);
        }

        internal HueButtonDevice Update(Device device, ZigbeeConnectivity? zigbeeConnectivity, DevicePower? devicePower, ButtonResource[] buttons)
        {
            Update(device, zigbeeConnectivity);

            DevicePowerResource = devicePower;
            Buttons = buttons
                .Select((button, i) => Buttons
                    .OfType<HueButton>()
                    .SingleOrDefault(d => d.ResourceId == button.Id)?.Update(button)
                        ?? new HueButton("Button " + (i + 1), button, this))
                .ToArray() ?? Array.Empty<HueButton>();

            return this;
        }
    }
}

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using MudBlazor;
using Q42.HueApi.Models;
using System;
using System.Linq;

namespace HomeBlaze.Philips.Hue
{
    public abstract class HueSwitchDevice : IThing, IIconProvider, ILastUpdatedProvider
    {
        public abstract string? Id { get; }

        internal Sensor Sensor { get; set; }

        public HueBridge Bridge { get; private set; }

        internal string ReferenceId => Sensor.Id;

        public string Title => Sensor.Name;

        public string IconName =>
            Buttons.Any(b => b.ButtonState != Abstractions.Inputs.ButtonState.None) ?
            Icons.Material.Filled.RadioButtonChecked :
            Icons.Material.Filled.RadioButtonUnchecked;

        public DateTimeOffset? LastUpdated => Sensor?.State.Lastupdated;

        [State]
        public int? ButtonState => Sensor?.State?.ButtonEvent;

        [State]
        public string? Type => Sensor?.Type;

        [State]
        public string? ManufacturerName => Sensor?.ManufacturerName;

        [State]
        public string? ModelId => Sensor?.ModelId;

        [State]
        public HueInputDeviceBase[] Buttons { get; protected set; } = Array.Empty<HueInputDeviceBase>();

        public HueSwitchDevice(Sensor sensor, HueBridge bridge)
        {
            Sensor = sensor;
            Bridge = bridge;
        }
    }
}

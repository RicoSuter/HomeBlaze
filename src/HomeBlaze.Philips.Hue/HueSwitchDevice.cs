using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HueApi.Models;
using MudBlazor;
using System;
using System.Linq;

namespace HomeBlaze.Philips.Hue
{
    public abstract class HueSwitchDevice : 
        IThing, 
        IIconProvider
        //ILastUpdatedProvider
    {
        public string Id => Bridge.Id + "/sensors/" + Sensor.Id;

        internal Device Sensor { get; set; }

        public HueBridge Bridge { get; private set; }

        internal Guid ReferenceId => Sensor.Id;

        public string Title => Sensor?.Metadata?.Name ?? "n/a";

        public string IconName =>
            Buttons.Any(b => b.ButtonState != Abstractions.Inputs.ButtonState.None) ?
            Icons.Material.Filled.RadioButtonChecked :
            Icons.Material.Filled.RadioButtonUnchecked;

        //public DateTimeOffset? LastUpdated => Sensor?.State.Lastupdated;

        //[State]
        //public int? ButtonState => Sensor?.State?.ButtonEvent;

        [State]
        public string? Type => Sensor?.Type;

        [State]
        public string? ManufacturerName => Sensor?.ProductData?.ManufacturerName;

        [State]
        public string? ModelId => Sensor?.ProductData?.ModelId;

        [State]
        public HueInputDeviceBase[] Buttons { get; protected set; } = Array.Empty<HueInputDeviceBase>();

        public HueSwitchDevice(Device sensor, HueBridge bridge)
        {
            Sensor = sensor;
            Bridge = bridge;
        }
    }
}

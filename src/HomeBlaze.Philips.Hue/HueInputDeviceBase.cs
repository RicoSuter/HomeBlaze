using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Inputs;
using HomeBlaze.Abstractions.Presentation;
using MudBlazor;
using Q42.HueApi.Models;
using System;
using System.Linq;

namespace HomeBlaze.Philips.Hue
{
    public class HueInputDeviceBase : IThing, IIconProvider, ILastUpdatedProvider,
        IButtonDevice
    {
        private readonly string _name;

        private ButtonState? _currentButtonState;
        private DateTimeOffset? _currentButtonChangeDate;
        private SensorInput _sensorInput;

        public string? Id => SwitchDevice != null ?
            "hue.input." + SwitchDevice.Bridge.BridgeId + "." + SwitchDevice.ReferenceId + "." + ReferenceId :
            null;

        public string Title => _name;

        public string IconName =>
            ButtonState != Abstractions.Inputs.ButtonState.None ?
            Icons.Material.Filled.RadioButtonChecked :
            Icons.Material.Filled.RadioButtonUnchecked;

        public HueSwitchDevice SwitchDevice { get; private set; }

        public int ReferenceId { get; private set; }

        public DateTimeOffset? LastUpdated { get; private set; }

        public DateTimeOffset? ButtonChangeDate => SwitchDevice?.Sensor?.State?.Lastupdated;

        [State]
        public ButtonState? ButtonState { get; private set; } = Abstractions.Inputs.ButtonState.None;

        internal ButtonState? InternalButtonState
        {
            get
            {
                var sensorEvent = _sensorInput?.Events
                    .FirstOrDefault(e => e.ButtonEvent == SwitchDevice?.Sensor?.State?.ButtonEvent);

                if (sensorEvent != null)
                {
                    if (sensorEvent.EventType == "initial_press")
                    {
                        return Abstractions.Inputs.ButtonState.Down;
                    }
                    else if (sensorEvent.EventType == "repeat")
                    {
                        return Abstractions.Inputs.ButtonState.Repeat;
                    }
                    else if (sensorEvent.EventType == "short_release")
                    {
                        return Abstractions.Inputs.ButtonState.Press;
                    }
                    else if (sensorEvent.EventType == "long_release")
                    {
                        return Abstractions.Inputs.ButtonState.LongPress;
                    }
                }

                return Abstractions.Inputs.ButtonState.None;
            }
        }

        public HueInputDeviceBase(int index, string name, SensorInput sensorInput, HueSwitchDevice switchDevice)
        {
            _name = name;
            _sensorInput = sensorInput;

            ReferenceId = index;
            SwitchDevice = switchDevice;

            Update(sensorInput);

            _currentButtonChangeDate = ButtonChangeDate;
            _currentButtonState = InternalButtonState;
        }

        internal HueInputDeviceBase Update(SensorInput sensorInput)
        {
            _sensorInput = sensorInput;
            LastUpdated = sensorInput != null ? DateTimeOffset.Now : null;
            RefreshButtonState();
            return this;
        }

        public void RefreshButtonState()
        {
            var newButtonChangeDate = ButtonChangeDate;
            var newButtonState = InternalButtonState;

            if ((newButtonChangeDate != _currentButtonChangeDate || newButtonState != _currentButtonState) &&
                newButtonState != Abstractions.Inputs.ButtonState.None)
            {
                _currentButtonChangeDate = newButtonChangeDate;
                _currentButtonState = newButtonState;

                if (newButtonState != Abstractions.Inputs.ButtonState.Repeat ||
                    ButtonState != Abstractions.Inputs.ButtonState.Repeat)
                {
                    ButtonState = newButtonState;

                    if (newButtonState != Abstractions.Inputs.ButtonState.None &&
                        newButtonState != Abstractions.Inputs.ButtonState.Down &&
                        newButtonState != Abstractions.Inputs.ButtonState.Repeat)
                    {
                        ButtonState = Abstractions.Inputs.ButtonState.None;
                    }
                }
            }
        }
    }
}

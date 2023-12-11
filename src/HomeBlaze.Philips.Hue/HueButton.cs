using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Inputs;
using HomeBlaze.Abstractions.Presentation;
using HueApi.Models;
using MudBlazor;
using System;

namespace HomeBlaze.Philips.Hue
{
    public class HueButton :
        IThing,
        IIconProvider,
        ILastUpdatedProvider,
        IButtonDevice
    {
        private readonly string _name;

        private ButtonState? _currentButtonState;
        private DateTimeOffset? _currentButtonChangeDate;

        internal ButtonResource ButtonResource { get; set; }

        public string Id => SwitchDevice.Bridge.Id + $"/inputs/{SwitchDevice.DeviceId}/buttons/{ReferenceId}";

        public string Title => _name;

        public string IconName =>
            ButtonState != Abstractions.Inputs.ButtonState.None ?
            Icons.Material.Filled.RadioButtonChecked :
            Icons.Material.Filled.RadioButtonUnchecked;

        public HueButtonDevice SwitchDevice { get; private set; }

        public Guid ReferenceId => ButtonResource.Id;

        public DateTimeOffset? LastUpdated { get; internal set; }

        public DateTimeOffset? ButtonChangeDate => ButtonResource.CreationTime;

        [State]
        public ButtonState? ButtonState { get; private set; } = Abstractions.Inputs.ButtonState.None;

        internal ButtonState? InternalButtonState
        {
            get
            {
                var sensorEvent = ButtonResource?.Button?.LastEvent;
                if (sensorEvent != null && sensorEvent.HasValue)
                {
                    var eventType = sensorEvent.Value;
                    if (eventType == ButtonLastEvent.initial_press)
                    {
                        return Abstractions.Inputs.ButtonState.Down;
                    }
                    else if (eventType == ButtonLastEvent.repeat)
                    {
                        return Abstractions.Inputs.ButtonState.Repeat;
                    }
                    else if (eventType == ButtonLastEvent.short_release)
                    {
                        return Abstractions.Inputs.ButtonState.Press;
                    }
                    else if (eventType == ButtonLastEvent.long_release)
                    {
                        return Abstractions.Inputs.ButtonState.LongPress;
                    }
                }

                return Abstractions.Inputs.ButtonState.None;
            }
        }

        public HueButton(string name, ButtonResource sensorInput, HueButtonDevice switchDevice)
        {
            _name = name;
            ButtonResource = sensorInput;

            SwitchDevice = switchDevice;

            Update(sensorInput);

            _currentButtonChangeDate = ButtonChangeDate;
            _currentButtonState = InternalButtonState;
        }

        internal HueButton Update(ButtonResource sensorInput)
        {
            ButtonResource = sensorInput;
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

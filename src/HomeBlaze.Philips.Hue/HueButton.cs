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
        private ButtonState? buttonState = Abstractions.Inputs.ButtonState.None;

        internal ButtonResource ButtonResource { get; set; }

        public string Id => ParentDevice.Bridge.Id + $"/devices/{ParentDevice.ResourceId}/buttons/{ResourceId}";

        public string Title => _name;

        public string IconName =>
            ButtonState != Abstractions.Inputs.ButtonState.None ?
            Icons.Material.Filled.RadioButtonChecked :
            Icons.Material.Filled.RadioButtonUnchecked;

        public HueButtonDevice ParentDevice { get; private set; }

        public Guid ResourceId => ButtonResource.Id;

        public DateTimeOffset? LastUpdated { get; internal set; }

        public DateTimeOffset? ButtonChangeDate { get; internal set; }

        [State]
        public ButtonState? ButtonState
        {
            get => buttonState; private set
            {
                Console.WriteLine($"Button {ResourceId}: " + value);
                buttonState=value;
            }
        }

        internal ButtonState? InternalButtonState
        {
            get
            {
                var lastEvent = ButtonResource?.Button?.LastEvent;
                if (lastEvent != null && lastEvent.HasValue)
                {
                    var eventType = lastEvent.Value;
                    return GetButtonState(eventType);
                }

                return Abstractions.Inputs.ButtonState.None;
            }
        }

        public static ButtonState GetButtonState(ButtonLastEvent eventType)
        {
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

            return Abstractions.Inputs.ButtonState.None;
        }

        public HueButton(string name, ButtonResource buttonResource, HueButtonDevice buttonDevice)
        {
            _name = name;

            ButtonResource = buttonResource;
            ParentDevice = buttonDevice;

            Update(buttonResource);

            _currentButtonChangeDate = ButtonChangeDate;
            _currentButtonState = InternalButtonState;
        }

        internal HueButton Update(ButtonResource buttonResource)
        {
            ButtonResource = buttonResource;
            LastUpdated = DateTimeOffset.Now;
            RefreshButtonState();
            return this;
        }

        public void RefreshButtonState()
        {
            var newButtonChangeDate = ButtonChangeDate;
            var newButtonState = InternalButtonState;

            if ((newButtonChangeDate != _currentButtonChangeDate || newButtonState != _currentButtonState) &&
                newButtonState != Abstractions.Inputs.ButtonState.None &&
                newButtonChangeDate != null)
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

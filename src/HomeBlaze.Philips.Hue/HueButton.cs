using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Inputs;
using HomeBlaze.Abstractions.Presentation;
using HueApi.Models;
using MudBlazor;
using System;
using System.Reactive.Subjects;

namespace HomeBlaze.Philips.Hue
{
    public class HueButton :
        IThing,
        IIconProvider,
        ILastUpdatedProvider,
        IButtonDevice,
        IObservable<Abstractions.Inputs.ButtonEvent>,
        IDisposable
    {
        private readonly string _name;
        private readonly Subject<Abstractions.Inputs.ButtonEvent> _buttonEventSubject = new();

        private ButtonState? _currentButtonState;
        private DateTimeOffset? _currentButtonChangeDate;

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

        public DateTimeOffset? ButtonChangeDate => ButtonResource?.Button?.ButtonReport?.Updated;

        [State]
        public ButtonState? ButtonState { get; private set; } = Abstractions.Inputs.ButtonState.None;

        internal ButtonState? InternalButtonState
        {
            get
            {
                var lastEvent = ButtonResource?.Button?.ButtonReport?.Event;
                if (lastEvent != null && lastEvent.HasValue)
                {
                    var eventType = lastEvent.Value;
                    return GetButtonState(eventType);
                }

                return Abstractions.Inputs.ButtonState.None;
            }
        }

        public static ButtonState GetButtonState(HueApi.Models.ButtonEvent eventType)
        {
            if (eventType == HueApi.Models.ButtonEvent.initial_press)
            {
                return Abstractions.Inputs.ButtonState.Down;
            }
            else if (eventType == HueApi.Models.ButtonEvent.repeat)
            {
                return Abstractions.Inputs.ButtonState.Repeat;
            }
            else if (eventType == HueApi.Models.ButtonEvent.short_release)
            {
                return Abstractions.Inputs.ButtonState.Press;
            }
            else if (eventType == HueApi.Models.ButtonEvent.long_release)
            {
                return Abstractions.Inputs.ButtonState.LongPress;
            }

            return Abstractions.Inputs.ButtonState.None;
        }

        public HueButton(string name, ButtonResource buttonResource, HueButtonDevice buttonDevice, bool initialization)
        {
            _name = name;

            ButtonResource = buttonResource;
            ParentDevice = buttonDevice;
            Update(buttonResource, initialization);

            _currentButtonChangeDate = ButtonChangeDate;
            _currentButtonState = InternalButtonState;
        }

        internal HueButton Update(ButtonResource buttonResource, bool initialization)
        {
            ButtonResource = buttonResource;
            LastUpdated = DateTimeOffset.Now;
            if (!initialization)
            {
                RefreshButtonState();
            }
            return this;
        }

        public void RefreshButtonState()
        {
            var newButtonChangeDate = ButtonChangeDate;
            var newButtonState = InternalButtonState;

            if (newButtonChangeDate != null &&
                newButtonChangeDate != _currentButtonChangeDate &&
                newButtonState != Abstractions.Inputs.ButtonState.None &&
                newButtonState != _currentButtonState)
            {
                _currentButtonChangeDate = newButtonChangeDate;
                _currentButtonState = newButtonState;

                if (newButtonState != Abstractions.Inputs.ButtonState.Repeat ||
                    ButtonState != Abstractions.Inputs.ButtonState.Repeat)
                {
                    ButtonState = newButtonState;

                    if (newButtonState.HasValue)
                    {
                        _buttonEventSubject.OnNext(new Abstractions.Inputs.ButtonEvent
                        {
                            ThingId = Id,
                            ButtonState = newButtonState.Value
                        });
                    }

                    if (newButtonState != Abstractions.Inputs.ButtonState.None &&
                        newButtonState != Abstractions.Inputs.ButtonState.Down &&
                        newButtonState != Abstractions.Inputs.ButtonState.Repeat)
                    {
                        // change back to none
                        ButtonState = Abstractions.Inputs.ButtonState.None;
                    }
                }
            }
        }

        public IDisposable Subscribe(IObserver<Abstractions.Inputs.ButtonEvent> observer)
        {
            return _buttonEventSubject.Subscribe(observer);
        }

        public void Dispose()
        {
            _buttonEventSubject.Dispose();
        }
    }
}

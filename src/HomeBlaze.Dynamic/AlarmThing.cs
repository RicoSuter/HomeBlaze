using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Abstractions.Services;
using System;
using System.Threading.Tasks;
using System.Threading;
using HomeBlaze.Services.Abstractions;
using HomeBlaze.Abstractions.Presentation;
using System.ComponentModel;
using HomeBlaze.Messages;
using HomeBlaze.Abstractions.Sensors;
using System.Collections.Generic;
using HomeBlaze.Abstractions.Devices.Light;

namespace HomeBlaze.Dynamic
{
    [DisplayName("Alarm")]
    public class AlarmThing : 
        AsyncEventListener, 
        IThing,
        IIconProvider, 
        IPresenceSensor
    {
        private readonly IThingManager _thingManager;
        private readonly TimeSpan _alarmTriggerDuration = TimeSpan.FromSeconds(30);

        public string IconName => "fa-solid fa-bell";

        public string IconColor => IsEnabled ? "Success" : "Error";

        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public string? Title { get; set; }

        [Configuration, State]
        public bool IsEnabled { get; set; } = true;

        [Configuration]
        public IList<string> PresenceThingIds { get; set; } = new List<string>();

        [Configuration]
        public string? NotificationPublisherId { get; set; }

        [Configuration]
        public string? BlinkingLightbulbId { get; set; }

        [State]
        public bool? IsPresent => PresenceTime != null;

        [State]
        public DateTimeOffset? PresenceTime { get; private set; }

        [State]
        public DateTimeOffset? AlarmTime { get; private set; }

        public AlarmThing(IEventManager eventManager, IThingManager thingManager)
            : base(eventManager)
        {
            _thingManager = thingManager;
        }

        [Operation]
        public void EnableAlarm()
        {
            IsEnabled = true;
            _thingManager.DetectChanges(this);
        }

        [Operation]
        public void DisableAlarm()
        {
            IsEnabled = false;
            CancelAlarm();
        }

        [Operation]
        public void CancelAlarm()
        {
            PresenceTime = null;
            AlarmTime = null;
            _thingManager.DetectChanges(this);
        }

        protected override async Task HandleMessageAsync(IEvent @event, CancellationToken cancellationToken)
        {
            if (!IsEnabled)
            {
                return;
            }
            else if (IsPresent != true)
            {
                await SearchIntruderAsync(@event, cancellationToken);
            }
            else if (DateTimeOffset.Now - PresenceTime > _alarmTriggerDuration && AlarmTime == null)
            {
                AlarmTime = DateTimeOffset.Now;
                _thingManager.DetectChanges(this);

                await TurnOnAlarmAsync(cancellationToken);
            }
        }

        private Task SearchIntruderAsync(IEvent @event, CancellationToken cancellationToken)
        {
            if (@event is ThingStateChangedEvent changedEvent &&
                PresenceThingIds.Contains(changedEvent.Thing.Id))
            {
                var isPresent = 
                    changedEvent.Thing is IPresenceSensor presenceSensor &&
                    presenceSensor.IsPresent == true;

                var isDoorOpen =
                    changedEvent.Thing is IDoorSensor doorSensor &&
                    doorSensor.DoorState == DoorState.Open;

                if (isPresent || isDoorOpen)
                {
                    PresenceTime = DateTimeOffset.Now;
                    _thingManager.DetectChanges(this);
                }
            }

            return Task.CompletedTask;
        }

        private async Task TurnOnAlarmAsync(CancellationToken cancellationToken)
        {
            var notificationPublisher = _thingManager.TryGetById(NotificationPublisherId) as INotificationPublisher;
            if (notificationPublisher != null)
            {
                await notificationPublisher.SendNotificationAsync("Intruder detected.", cancellationToken);
            }

            var lightbulb = _thingManager.TryGetById(BlinkingLightbulbId) as ILightbulb;
            if (lightbulb != null)
            {
                var dimmerLightbulb = lightbulb as IDimmerLightbulb;

#pragma warning disable CS4014

                Task.Run(async () =>
                {
                    lightbulb.TurnOnAsync(cancellationToken);

                    if (dimmerLightbulb != null)
                    {
                        dimmerLightbulb.DimmAsync(100, cancellationToken);
                    }

                    for (int i = 0; i < 120; i++)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2.5), cancellationToken);
                        lightbulb.TurnOffAsync(cancellationToken);

                        await Task.Delay(TimeSpan.FromSeconds(2.5), cancellationToken);
                        lightbulb.TurnOnAsync(cancellationToken);

                        if (AlarmTime == null)
                        {
                            break;
                        }
                    }
                }, cancellationToken);

#pragma warning restore CS4014 
            }
        }
    }
}
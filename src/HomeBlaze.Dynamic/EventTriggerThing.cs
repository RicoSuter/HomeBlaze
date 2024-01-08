using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using HomeBlaze.Services.Abstractions;
using System.ComponentModel;
using HomeBlaze.Components.Editors;
using Microsoft.Extensions.Logging;

namespace HomeBlaze.Dynamic
{
    [DisplayName("Event Trigger")]
    [ThingSetup(typeof(EventTriggerThingSetup), CanEdit = true, CanClone = true)]
    public class EventTriggerThing : ExtensionThing
    {
        private readonly IThingManager _thingManager;
        private readonly ILogger<EventTriggerThing> _logger;

        public EventTriggerThing(IThingManager thingManager, IEventManager eventManager, ILogger<EventTriggerThing> logger)
            : base(thingManager, eventManager)
        {
            _thingManager = thingManager;
            _logger = logger;
        }

        [Configuration]
        public string? EventTypeName { get; set; }

        [Configuration]
        public IList<Operation> Operations { get; set; } = new List<Operation>();

        [Configuration, State]
        public bool IsEnabled { get; set; } = true;

        [State]
        public bool IsTriggered { get; private set; }

        [State]
        public DateTimeOffset? LastExecution { get; private set; }

        protected override async Task HandleMessageAsync(IEvent @event, CancellationToken cancellationToken)
        {
            if (EventTypeName is not null &&
                @event is IThingEvent thingEvent &&
                thingEvent.ThingId == ExtendedThingId &&
                thingEvent.GetType().FullName == EventTypeName)
            {
                Execute();
            }

            await base.HandleMessageAsync(@event, cancellationToken);
        }

        private void Execute()
        {
            IsTriggered = true;
            LastExecution = DateTimeOffset.Now;

            _thingManager.DetectChanges(this);

            // TODO: Should this block message execution?
            Task.Run(async () =>
            {
                _logger.LogInformation("Executing trigger operations.");

                foreach (var operation in Operations)
                {
                    await operation.ExecuteAsync(_thingManager, _logger, CancellationToken.None);
                }

                _logger.LogInformation("Trigger operations executed.");
            });
        }
    }
}
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using HomeBlaze.Services.Abstractions;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Messages;
using System.ComponentModel;
using HomeBlaze.Components.Editors;
using MudBlazor;

namespace HomeBlaze.Dynamic
{
    [DisplayName("Trigger")]
    [ThingSetup(typeof(TriggerThingSetup), CanEdit = true, CanClone = true)]
    public class TriggerThing : AsyncEventListener, IThing, IIconProvider
    {
        private readonly IThingManager _thingManager;
        private readonly ILogger<DynamicThing> _logger;

        public string IconName => "fa-solid fa-bolt";

        public string IconColor => IsEnabled ? "Success" : "Error";

        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public string? Title { get; set; }

        [Configuration]
        public IList<PropertyCondition> Conditions { get; set; } = new List<PropertyCondition>();

        [Configuration]
        public IList<Operation> Operations { get; set; } = new List<Operation>();

        [Configuration, State]
        public bool IsEnabled { get; set; } = true;

        [State]
        public bool IsTriggered { get; private set; }

        [State]
        public DateTimeOffset? LastExecution { get; private set; }

        public TriggerThing(IThingManager thingManager, IEventManager eventManager, ILogger<DynamicThing> logger)
            : base(eventManager)
        {
            _thingManager = thingManager;
            _logger = logger;
        }

        protected override Task HandleMessageAsync(IEvent @event, CancellationToken cancellationToken)
        {
            if (!IsEnabled)
            {
                return Task.CompletedTask;
            }

            if (@event is ThingStateChangedEvent stateChangedEvent)
            {
                if (Conditions.Any(v => v.ThingId == stateChangedEvent.Thing.Id &&
                                        v.PropertyName == stateChangedEvent.PropertyName))
                {
                    foreach (var conditions in Conditions
                        .Where(v => v.ThingId == stateChangedEvent.Thing.Id &&
                                    v.PropertyName == stateChangedEvent.PropertyName))
                    {
                        conditions.Apply(_thingManager, stateChangedEvent);
                    }

                    Evaluate();
                }
            }
            else if (@event is TimerEvent && Conditions.Any(c => c.MinimumHoldDuration > TimeSpan.Zero))
            {
                Evaluate();
            }

            return Task.CompletedTask;
        }

        private void Evaluate()
        {
            if (Conditions.All(c => c.Result))
            {
                if (!IsTriggered)
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
            else if (IsTriggered)
            {
                IsTriggered = false;
                _thingManager.DetectChanges(this);
            }
        }
    }
}
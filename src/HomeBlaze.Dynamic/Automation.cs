using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Components.Editors;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.Logging;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Dynamic
{
    [ThingSetup(typeof(AutomationSetup), CanEdit = true, CanClone = true)]
    public class Automation : AsyncEventListener, IThing, IIconProvider, IJsonOnDeserialized
    {
        internal const string IdleStateName = "Idle";

        private bool _forceEvaluation;
        private readonly IThingManager _thingManager;
        private readonly ILogger<Automation> _logger;

        public string IconName => "fas fa-microchip";

        public Color IconColor => IsEnabled ? Color.Success : Color.Error;

        [Configuration(IsIdentifier = true)]
        public string? Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public string? Title { get; set; } = "MyAutomation";

        [Configuration, State]
        public bool IsEnabled { get; set; } = true;

        [Configuration]
        public string InitialState { get; set; } = IdleStateName;

        [State]
        public string State { get; internal set; }

        [State]
        public DateTimeOffset LastStateChange { get; set; }

        [Configuration]
        public List<AutomationState> States { get; set; } = new List<AutomationState>
        {
            new AutomationState { Name = IdleStateName, X = 50, Y = 50 }
        };

        [Configuration]
        public List<AutomationTransition> Transitions { get; set; } = new List<AutomationTransition>();

#pragma warning disable CS8618 // State not null ensured in OnDeserialized

        public Automation(IThingManager thingManager, IEventManager eventManager, ILogger<Automation> logger)
            : base(eventManager)
        {
            _thingManager = thingManager;
            _logger = logger;
        }

        public void OnDeserialized()
        {
            SetState(InitialState);
        }

#pragma warning restore CS8618

        protected override Task HandleMessageAsync(IEvent @event, CancellationToken cancellationToken)
        {
            if (!IsEnabled)
            {
                return Task.CompletedTask;
            }

            try
            {
                foreach (var transition in Transitions.Where(t => t.FromState == State))
                {
                    if (transition.Condition.Evaluate(this, @event, _thingManager, _forceEvaluation) == true)
                    {
                        var state = States.FirstOrDefault(s => s.Name == transition.ToState);
                        if (state?.Name != null)
                        {
                            SetState(state.Name);
                           
                            // TODO: Should this block message execution?
                            Task.Run(async () =>
                            {
                                foreach (var operation in transition.Operations)
                                {
                                    await operation.ExecuteAsync(_thingManager, _logger, CancellationToken.None);
                                }

                                foreach (var operation in state.Operations)
                                {
                                    await operation.ExecuteAsync(_thingManager, _logger, CancellationToken.None);
                                }
                            });
                        }
                        else
                        {
                            _logger.LogError("State {StateName} is defined in transition but could not be found in thing {ThingId}.", transition.ToState, Id);
                        }

                        // cancel after first transition
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to process message.");
            }

            _forceEvaluation = false;
            return Task.CompletedTask;
        }

        [MemberNotNull(nameof(State))]
        internal void SetState(string stateName)
        {
            var now = DateTimeOffset.Now;

            State = stateName;
            LastStateChange = DateTimeOffset
                .FromUnixTimeSeconds(now.ToUnixTimeSeconds())
                .ToOffset(now.Offset);

            _forceEvaluation = true;
            _thingManager.DetectChanges(this);
        }
    }
}

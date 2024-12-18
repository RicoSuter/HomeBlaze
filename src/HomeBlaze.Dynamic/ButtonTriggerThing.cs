using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.ComponentModel;
using System.Linq;

using Namotion.Devices.Abstractions.Messages;

using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using HomeBlaze.Components.Editors;
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Inputs;
using Namotion.Devices.Abstractions.Inputs;
using HomeBlaze.Abstractions.Presentation;

namespace HomeBlaze.Dynamic
{
    [DisplayName("Button Trigger")]
    public class ButtonTriggerThing : 
        ExtensionThing, 
        IIconProvider
    {
        private readonly IThingManager _thingManager;
        private readonly ILogger<ButtonTriggerThing> _logger;

        string IIconProvider.IconName => "fa-solid fa-circle-dot";

        [Configuration]
        public IList<Operation> Operations { get; set; } = [];

        [Configuration, State]
        public bool IsEnabled { get; set; } = true;

        [State]
        public bool IsTriggered { get; private set; }

        [State]
        public DateTimeOffset? LastExecution { get; private set; }

        public ButtonTriggerThing(IThingManager thingManager, IEventManager eventManager, ILogger<ButtonTriggerThing> logger)
            : base(thingManager, eventManager)
        {
            _thingManager = thingManager;
            _logger = logger;
        }

        [Operation]
        public void Execute()
        {
            // TODO: Guard this
            
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

        public static bool CanExtend(IThing thing)
        {
            return thing
                .GetType()
                .GetInterfaces()
                .Any(a => a.Name == "IObservable`1" && 
                          a.GenericTypeArguments[0].IsAssignableTo(typeof(ButtonEvent)));
        }

        protected override async Task HandleMessageAsync(IEvent @event, CancellationToken cancellationToken)
        {
            if (@event is ButtonEvent buttonEvent &&
                (buttonEvent.Source as IThing)?.Id == ExtendedThingId &&
                buttonEvent.ButtonState == ButtonState.Release)
            {
                Execute();
            }

            await base.HandleMessageAsync(@event, cancellationToken);
        }
    }
}
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Messages;
using Namotion.Devices.Abstractions.Messages;

namespace HomeBlaze.Services.Abstractions
{
    public abstract class ExtensionThing<TFor> : ExtensionThing
       where TFor : IThing
    {
        protected ExtensionThing(IThingManager thingManager, IEventManager eventManager)
            : base(thingManager, eventManager)
        {
        }

        new public TFor? ExtendedThing => (TFor?)base.ExtendedThing;
    }

    public abstract class ExtensionThing : AsyncEventListener, IThing, IExtensionThing, IDisposable
    {
        private readonly IThingManager _thingManager;

        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public string? Title { get; set; }

        [Configuration]
        public string? ExtendedThingId { get; set; }

        public IThing? ExtendedThing => _thingManager.TryGetById(ExtendedThingId);

        public ExtensionThing(IThingManager thingManager, IEventManager eventManager)
            : base(eventManager)
        {
            _thingManager = thingManager;
        }

        protected override Task HandleMessageAsync(IEvent @event, CancellationToken cancellationToken)
        {
            if (@event is ThingStateChangedEvent stateChangedEvent &&
                (stateChangedEvent.Source as IThing)?.Id == ExtendedThingId)
            {
                _thingManager.DetectChanges(this);
            }

            return Task.CompletedTask;
        }
    }
}
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using HomeBlaze.Services.Abstractions;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Messages;
using System;
using HomeBlaze.Abstractions;
using System.ComponentModel;
using HomeBlaze.Components.Editors;

namespace HomeBlaze.Dynamic
{
    [DisplayName("Dynamic Aggregator")]
    public class DynamicAggregatorThing : AsyncEventListener, IIconProvider, IThing
    {
        private readonly IThingManager _thingManager;
        private readonly ITypeManager _typeManager;

        private Type? _interfaceType;

        public string IconName => "fa-solid fa-calculator";

        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public string? Title { get; set; }

        [Configuration]
        public string? InterfaceName { get; set; }

        [Configuration]
        public string? PropertyName { get; set; }

        [Configuration]
        public AggregationType Aggregation { get; set; }

        [State]
        public decimal? Value { get; set; }

        public DynamicAggregatorThing(IThingManager thingManager, ITypeManager typeManager,
            IEventManager eventManager, ILogger<DynamicAggregatorThing> logger)
            : base(eventManager)
        {
            _thingManager = thingManager;
            _typeManager = typeManager;
        }

        protected override Task HandleMessageAsync(IEvent @event, CancellationToken cancellationToken)
        {
            if (@event is ThingStateChangedEvent stateChangedEvent)
            {
                if (_interfaceType == null || _interfaceType.FullName != InterfaceName)
                {
                    _interfaceType = _typeManager.ThingInterfaces
                        .FirstOrDefault(i => i.FullName == InterfaceName);
                }

                if (_interfaceType != null && stateChangedEvent.Thing.GetType().IsAssignableTo(_interfaceType))
                {
                    var values = _thingManager
                        .AllThings
                        //.Where(t => _thingManager.TryGetMetadata()) // TODO(perf): Add interfaces to metadata for fast lookup
                        .Where(t => t.GetType().IsAssignableTo(_interfaceType))
                        .Select(t => _thingManager.TryGetPropertyState(t.Id, PropertyName, true)?.Value)
                        .Where(v => v != null)
                        .Select(Convert.ToDecimal);

                    switch (Aggregation)
                    {
                        case AggregationType.Average:
                            Value = values.Min();
                            break;

                        case AggregationType.Minimum:
                            Value = values.Min();
                            break;

                        case AggregationType.Maximum:
                            Value = values.Min();
                            break;

                        case AggregationType.Sum:
                            Value = values.Sum();
                            break;

                        case AggregationType.Count:
                            Value = values.Count();
                            break;

                        default:
                            return Task.CompletedTask;
                    }

                    _thingManager.DetectChanges(this);
                }

            }

            return Task.CompletedTask;
        }
    }
}
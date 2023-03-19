using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Namotion.Reflection;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace HomeBlaze.Services
{
    public class ThingManager : BackgroundService, IThingManager
    {
        private readonly BlockingCollection<IThing> _changeDetectionTriggers = new BlockingCollection<IThing>();
        private readonly HashSet<IThing> _thingsMarkedForChangeDetection = new HashSet<IThing>();

        private static readonly ConcurrentDictionary<Type, Tuple<MethodInfo, OperationAttribute>[]> _operationCache
            = new ConcurrentDictionary<Type, Tuple<MethodInfo, OperationAttribute>[]>();

        private readonly Dictionary<IThing, ThingMetadata> _things = new Dictionary<IThing, ThingMetadata>();

        private readonly IThingStorage _thingStorage;
        private readonly ITypeManager _typeManager;
        private readonly IEventManager _eventManager;
        private readonly ILogger<ThingManager> _logger;

        public ThingManager(IThingStorage thingLoader, ITypeManager typeManager, IEventManager eventManager, ILogger<ThingManager> thingManager)
        {
            _thingStorage = thingLoader;
            _typeManager = typeManager;
            _eventManager = eventManager;
            _logger = thingManager;
        }

        public IGroupThing? RootThing { get; private set; }

        public IEnumerable<IThing> AllThings
        {
            get
            {
                lock (_things)
                {
                    return _things.Keys.ToList();
                }
            }
        }

        public IThing? TryGetById(string? thingId)
        {
            if (thingId == null)
            {
                return null;
            }

            lock (_things)
            {
                return _things.Keys
                    .Where(t => t.Id == thingId)
                    .FirstOrDefault();
            }
        }

        public PropertyState? TryGetPropertyState(string? thingId, string? property, bool includeExtensions)
        {
            lock (_things)
            {
                if (property == null)
                {
                    return null;
                }

                var state = GetState(thingId, includeExtensions: false);
                if (state.TryGetValue(property, out var result) == true)
                {
                    return result;
                }
                else if (includeExtensions)
                {
                    state = GetExtensionState(thingId);
                    if (state.TryGetValue(property, out var result2) == true)
                    {
                        return result2;
                    }
                }

                return null;
            }
        }

        public IThing? TryGetParent(IThing thing)
        {
            lock (_things)
            {
                return _things
                    .Where(t => t.Value.Children.Contains(thing))
                    .Select(t => t.Key)
                    .FirstOrDefault();
            }
        }

        public IReadOnlyDictionary<string, PropertyState> GetState(string? thingId, bool includeExtensions)
        {
            lock (_things)
            {
                var thing = TryGetById(thingId);
                if (thing == null)
                {
                    return new Dictionary<string, PropertyState>();
                }

                var state = TryGetMetadata(thing)?
                    .CurrentFullState?
                    .ToDictionary(p => p.Key, p => p.Value) ??
                    new Dictionary<string, PropertyState>();

                if (includeExtensions)
                {
                    state = state
                        .Concat(GetExtensionState(thingId))
                        .ToDictionary(p => p.Key, p => p.Value);
                }

                return state;
            }
        }

        private Dictionary<string, PropertyState> GetExtensionState(string? thingId)
        {
            var state = new Dictionary<string, PropertyState>();
          
            lock (_things)
            {
                var thing = TryGetById(thingId);
                foreach (var extensionThing in _things.Keys
                    .Where(t => t is IExtensionThing extensionThing && extensionThing.ExtendedThing == thing))
                {
                    // TODO: What to do when multiple extensions have the same property?
                    // (eg two dynamicextensionthings with a thing property each?)

                    state = state
                        .Concat(GetState(extensionThing.Id, true))
                        .DistinctBy(p => p.Key) // TODO(docs): Currently extension things cannot overwrite values of the extended thing
                        .ToDictionary(p => p.Key, p => p.Value);
                }

            }

            return state;
        }

        public ThingOperation[] GetOperations(string? thingId, bool includeExtensions)
        {
            var thing = TryGetById(thingId);
            if (thing == null)
            {
                return Array.Empty<ThingOperation>();
            }

            var type = thing.GetType();
            if (!_operationCache.ContainsKey(type))
            {
                _operationCache[type] = thing.GetType()
                    .GetInterfaces()
                    .SelectMany(t => t.GetMethods())
                    .Concat(thing.GetType().GetRuntimeMethods())
                    .Select(m => new Tuple<MethodInfo, OperationAttribute>(m, m.GetCustomAttribute<OperationAttribute>(true)!))
                    .Where(m => m.Item2 != null)
                    .GroupBy(m => m.Item1.Name)
                    .Select(g => g.Last())
                    .ToArray();
            }

            var operations = _operationCache[type]
                .Select(o => new ThingOperation(
                    thing,
                    o.Item1.Name.Replace("Async", ""),
                    o.Item1.GetXmlDocsSummary(),
                    o.Item1,
                    o.Item2!));

            if (includeExtensions)
            {
                if (includeExtensions)
                {
                    ICollection<IThing> things;
                    lock (_things)
                    {
                        things = _things.Keys.ToArray();
                    }

                    foreach (var extensionThing in things
                        .Where(t => t is IExtensionThing extensionThing && extensionThing.ExtendedThing == thing))
                    {
                        operations = operations.Concat(GetOperations(extensionThing.Id, true));
                    }
                }
            }

            return operations.ToArray();
        }

        public async Task WriteConfigurationAsync(CancellationToken cancellationToken)
        {
            if (RootThing != null)
            {
                // TODO: This should schedule a write event and debounce & retry write request
                await _thingStorage.WriteRootThingAsync(RootThing, cancellationToken);
            }
        }

        public void DetectChanges(IThing thing)
        {
            lock (_thingsMarkedForChangeDetection)
            {
                _thingsMarkedForChangeDetection.Add(thing);
            }

            _changeDetectionTriggers.Add(thing);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _typeManager.InitializeAsync(stoppingToken);

            RootThing = await _thingStorage.ReadRootThingAsync(stoppingToken);

            Register(RootThing);
            DetectChanges(RootThing);

            _eventManager.Publish(new RootThingLoadedEvent(RootThing));

            ProcessThingChangeDetectionQueue(stoppingToken);
        }

        private void ProcessThingChangeDetectionQueue(CancellationToken stoppingToken)
        {
            var checkedThings = new List<IThing>();
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_changeDetectionTriggers.TryTake(out var _, 1000, stoppingToken))
                {
                    IThing[] things;

                    lock (_thingsMarkedForChangeDetection)
                    {
                        things = _thingsMarkedForChangeDetection.ToArray();
                        _thingsMarkedForChangeDetection.Clear();
                    }

                    checkedThings.Clear();
                    foreach (var thing in things)
                    {
                        try
                        {
                            DetectChanges(thing, checkedThings);
                        }
                        catch (Exception e)
                        {
                            _logger.LogWarning(e, "Failed to detect changes of thing {ThingId}.", thing.Id);
                        }
                    }
                }
            }
        }

        IThingMetadata? IThingManager.TryGetMetadata(IThing thing)
        {
            return TryGetMetadata(thing);
        }

        private ThingMetadata? TryGetMetadata(IThing thing)
        {
            lock (_things)
            {
                return _things.TryGetValue(thing, out var metadata) ? metadata : null;
            }
        }

        private void DetectChanges(IThing thing, List<IThing> checkedThings)
        {
            if (checkedThings.Contains(thing))
            {
                return;
            }

            checkedThings.Add(thing);

            var now = DateTimeOffset.Now;
            lock (_things)
            {
                var metadata = TryGetMetadata(thing);
                if (metadata != null)
                {
                    var newChildThings = new List<IThing>();
                    var newState = new Dictionary<string, PropertyState>();
                    var lastUpdated = thing is ILastUpdatedProvider lastUpdatedProvider ? lastUpdatedProvider.LastUpdated : null;

                    LoadState(thing, thing, thing.GetType(), newState, newChildThings, string.Empty, lastUpdated);
                    HandleChildThings(thing, _things[thing].Children, newChildThings);

                    foreach (var newPair in newState)
                    {
                        PropertyState oldState = default;
                        metadata.CurrentFullState?.TryGetValue(newPair.Key, out oldState);

                        if (!(newPair.Value.Value is IThing) &&
                            !(newPair.Value.Value is IEnumerable<IThing>) &&
                            (metadata.CurrentFullState == null || !Equals(oldState.Value, newPair.Value.Value)))
                        {
                            var message = new ThingStateChangedEvent(thing)
                            {
                                ChangeDate = now,

                                PropertyName = newPair.Key,

                                OldValue = oldState.Value,
                                NewValue = newPair.Value.Value
                            };

                            _eventManager.Publish(message);
                        }
                    }

                    metadata.CurrentFullState = newState;

                    foreach (var child in _things[thing].Children)
                    {
                        if (!checkedThings.Contains(child))
                        {
                            DetectChanges(child);
                        }
                    }
                }
            }
        }

        private void Register(IThing thing)
        {
            lock (_things)
            {
                if (_things.ContainsKey(thing))
                {
                    return;
                }

                _things[thing] = new ThingMetadata
                {
                    ThingSetupAttribute = thing.GetType().GetCustomAttribute<ThingSetupAttribute>(),
                };
            }

            _eventManager.Publish(new ThingRegisteredEvent(thing));

            if (thing is IHostedService hostedService)
            {
                // TODO: Why is task needed?
                Task.Run(async () => await hostedService.StartAsync(CancellationToken.None));
            }

            _logger.LogInformation("Thing {ThingId} registered.", thing.Id);
        }

        private void Unregister(IThing thing)
        {
            IThing[] children;

            lock (_things)
            {
                if (_things.Any(t => t.Value.Children.Contains(thing)))
                {
                    // still referenced from a thing
                    return;
                }

                children = _things[thing]
                    .Children
                    .Where(c => c != null)
                    .ToArray()!;

                _things.Remove(thing);
            }

            UnregisterChildren(thing, children);

            Task.Run(async () => await StopAndDisposeThingAsync(thing));

            _eventManager.Publish(new ThingUnregisteredEvent(thing));
            _logger.LogInformation("Thing {ThingId} unregistered.", thing.Id);
        }

        private async Task StopAndDisposeThingAsync(IThing thing)
        {
            try
            {
                if (thing is IHostedService hostedService)
                {
                    await hostedService.StopAsync(CancellationToken.None);
                }
            }
            finally
            {
                if (thing is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }

                if (thing is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                _logger.LogInformation("Thing {ThingId} stopped.", thing.Id);
            }
        }

        private void UnregisterChildren(IThing thing, IThing[] children)
        {
            foreach (var child in children)
            {
                try
                {
                    Unregister(child);
                }
                catch (Exception)
                {
                    _logger.LogError("Failed to unregister child thing {ThingId}.", thing.Id);
                }
            }
        }

        private void LoadState(IThing rootThing, object? obj, Type type,
            Dictionary<string, PropertyState> state,
            List<IThing> childThings,
            string prefix,
            DateTimeOffset? lastUpdated)
        {
            var metadata = TryGetMetadata(rootThing);
            foreach (var property in type.GetStateProperties())
            {
                var stateAttribute = property.ContextAttributes.OfType<StateAttribute>().FirstOrDefault();
                var scanForStateAttribute = property.ContextAttributes.OfType<ScanForStateAttribute>().FirstOrDefault();
                var parentThingAttribute = property.ContextAttributes.OfType<ParentThingAttribute>().FirstOrDefault();

                var newValue = ReflectionUtilities.TryGetPropertyValue(obj, property, _logger);

                if (stateAttribute != null)
                {
                    var propertyName = prefix + stateAttribute.GetPropertyName(rootThing, property);

                    var oldState = default(PropertyState);
                    metadata?.CurrentFullState?.TryGetValue(propertyName, out oldState);

                    var newState = new PropertyState
                    {
                        Name = propertyName,
                        SourceThing = rootThing,

                        Value = newValue,
                        PreviousValue = Equals(oldState.Value, newValue) ? oldState.PreviousValue : oldState.Value,

                        Attribute = stateAttribute,
                        Property = property
                    };

                    if (property.PropertyType.Type.IsAssignableTo(typeof(IThing))) // TODO: cache these checks?
                    {
                        var newThing = newValue as IThing;
                        if (newThing != null)
                        {
                            childThings.Add(newThing);
                        }

                        state[propertyName] = newState;
                    }
                    else if (property.PropertyType.Type.IsAssignableTo(typeof(IEnumerable<IThing>)))
                    {
                        var newThings = newValue as IEnumerable<IThing>;
                        if (newThings != null)
                        {
                            childThings.AddRange(newThings);
                        }

                        state[propertyName] = newState;
                    }
                    else
                    {
                        if (obj is ILastUpdatedProvider lastUpdatedProvider)
                        {
                            lastUpdated = lastUpdatedProvider.LastUpdated;
                        }

                        newState.LastUpdated = lastUpdated;
                        newState.LastChanged = Equals(newValue, oldState.Value) ? oldState.LastChanged : lastUpdated;

                        state[propertyName] = newState;
                    }
                }
                else if (scanForStateAttribute != null)
                {
                    if (newValue is IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            LoadState(rootThing, item, item.GetType(), state, childThings, prefix, lastUpdated);
                        }
                    }
                    else
                    {
                        LoadState(rootThing, newValue, property.PropertyType, state, childThings, prefix, lastUpdated);
                    }
                }
                else if (parentThingAttribute != null && newValue == null)
                {
                    // TODO: Is this the right place and a good idea here?
                    property.SetValue(obj, TryGetParent(rootThing));
                }
            }

            if (obj is IStateProvider stateProvider)
            {
                foreach (var keyValuePair in stateProvider.GetState())
                {
                    var oldState = default(PropertyState);
                    metadata?.CurrentFullState?.TryGetValue(keyValuePair.Key, out oldState);

                    state[keyValuePair.Key] = new PropertyState
                    {
                        Name = keyValuePair.Key,
                        SourceThing = rootThing,

                        Value = keyValuePair.Value,
                        PreviousValue = Equals(oldState.Value, keyValuePair.Value) ? oldState.PreviousValue : oldState.Value,
                    };
                }
            }
        }

        private void HandleChildThings(IThing parent, IEnumerable<IThing>? oldThings, IEnumerable<IThing>? newThings)
        {
            var addedThings = newThings?
                .Where(t => oldThings?.Any(x => x == t) != true)
                .ToArray() ??
                Enumerable.Empty<IThing>();

            var removedThings = oldThings?
                .Where(t => newThings?.Any(x => x == t) != true)
                .ToArray() ??
                Enumerable.Empty<IThing>();

            foreach (var thing in addedThings)
            {
                _things[parent].Children.Add(thing);
                Register(thing);
            }

            foreach (var thing in removedThings)
            {
                _things[parent].Children.Remove(thing);
                Unregister(thing);
            }
        }
    }
}
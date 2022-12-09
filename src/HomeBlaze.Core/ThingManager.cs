using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Namotion.Reflection;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace HomeBlaze
{
    public class ThingManager : BackgroundService, IThingManager
    {
        private readonly BlockingCollection<IThing> _changeDetectionTriggers = new BlockingCollection<IThing>();
        private readonly ConcurrentDictionary<IThing, bool> _thingsMarkedForChangeDetection = new ConcurrentDictionary<IThing, bool>();

        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();
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

        public IThing? RootThing { get; private set; }

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
            lock (_things)
            {
                return _things.Keys
                    .Where(t => t.Id == thingId)
                    .FirstOrDefault();
            }
        }

        public PropertyState? TryGetPropertyState(string thingId, string property, bool includeExtensions)
        {
            lock (_things)
            {
                var thing = TryGetById(thingId);
                if (thing != null)
                {
                    var state = GetState(thing, includeExtensions);
                    if (state.TryGetValue(property, out var result) == true)
                    {
                        return result;
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

        public IReadOnlyDictionary<string, PropertyState> GetState(IThing thing, bool includeExtensions)
        {
            lock (_things)
            {
                var state = TryGetMetadata(thing)?
                    .CurrentFullState?
                    .ToDictionary(p => p.Key, p => p.Value) ??
                    new Dictionary<string, PropertyState>();

                if (includeExtensions)
                {
                    // TODO: Improve merge performance
                    foreach (var extensionThing in _things.Keys
                        .Where(t => t is IExtensionThing extensionThing && extensionThing.ExtendedThing == thing))
                    {
                        // TODO: What to do when multiple extensions with same property? (eg two dynamicextensionthings with a thing property each?)
                        state = state
                            .Concat(GetState(extensionThing, true))
                            .DistinctBy(p => p.Key) // TODO(docs): Currently extension things cannot overwrite values of the extended thing
                            .ToDictionary(p => p.Key, p => p.Value);
                    }
                }

                return state;
            }
        }

        public ThingOperation[] GetOperations(IThing thing, bool includeExtensions)
        {
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
                        operations = operations.Concat(GetOperations(extensionThing, true));
                    }
                }
            }

            return operations.ToArray();
        }

        public async Task WriteConfigurationAsync(CancellationToken cancellationToken)
        {
            if (RootThing != null)
            {
                await _thingStorage.WriteThingAsync(RootThing, cancellationToken);
            }
        }

        public void DetectChanges(IThing thing)
        {
            _thingsMarkedForChangeDetection[thing] = true;
            _changeDetectionTriggers.Add(thing);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _typeManager.InitializeAsync(stoppingToken);

            RootThing = await _thingStorage.ReadThingAsync(stoppingToken);

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
                    var things = _thingsMarkedForChangeDetection.Keys.ToArray();
                    _thingsMarkedForChangeDetection.Clear();

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
                children = _things[thing]
                    .Children
                    .Where(c => c != null)
                    .ToArray()!;

                _things.Remove(thing);
            }

            UnregisterChildren(thing, children);

            Task.Run(async () => await StopAndDisposeThingAsync(thing));

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
            foreach (var property in GetStateProperties(type))
            {
                var stateAttribute = property.GetCustomAttribute<StateAttribute>(true);
                var scanForStateAttribute = property.GetCustomAttribute<ScanForStateAttribute>(true);
                var parentThingAttribute = property.GetCustomAttribute<ParentThingAttribute>(true);
                
                var newValue = TryGetPropertyValue(obj, property);

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
                        Property = property.ToContextualProperty()
                    };

                    if (property.PropertyType.IsAssignableTo(typeof(IThing))) // TODO: cache these checks?
                    {
                        var newThing = newValue as IThing;
                        if (newThing != null)
                        {
                            childThings.Add(newThing);
                        }

                        state[propertyName] = newState;
                    }
                    else if (property.PropertyType.IsAssignableTo(typeof(IEnumerable<IThing>)))
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
                else if (parentThingAttribute != null && newValue != null)
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

        private object? TryGetPropertyValue(object? obj, PropertyInfo property)
        {
            try
            {
                return obj != null ? property.GetValue(obj) : null;
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Getter of property threw an exception.", e);
                return null;
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

        private static PropertyInfo[] GetStateProperties(Type type)
        {
            // TODO: Use namotion.reflection types
            if (!_propertyCache.TryGetValue(type, out var pair))
            {
                var interfaceProperties = type
                    .GetInterfaces()
                    .SelectMany(i => i.GetProperties()
                        .Where(property => property.GetCustomAttribute<StateAttribute>(true) != null))
                    .ToArray();

                var properties = type
                    .GetRuntimeProperties()
                    .Where(property => property.GetCustomAttribute<StateAttribute>(true) != null ||
                                       property.GetCustomAttribute<ScanForStateAttribute>(true) != null ||
                                       property.GetCustomAttribute<ParentThingAttribute>(true) != null)
                    .ToArray();

                _propertyCache[type] = interfaceProperties
                    .Concat(properties)
                    .DistinctBy(p => p.Name)
                    .ToArray();
            }

            return _propertyCache[type];
        }
    }
}
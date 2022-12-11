using Castle.DynamicProxy;
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Abstractions.Services;
using Jint;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using HomeBlaze.Services.Abstractions;

namespace HomeBlaze.Dynamic
{
    public class Property
    {
        public string? Name { get; set; }

        public string? Expression { get; set; }
    }

    [ThingSetup(typeof(DynamicExtensionThingSetup), CanEdit = true)]
    public class DynamicExtensionThing : ExtensionThing, IInterceptor
    {
        public class DynamicStateAttribute : StateAttribute
        {
            public override string GetPropertyName(IThing thing, PropertyInfo property)
            {
                return ((DynamicExtensionThing)thing).Property ?? base.GetPropertyName(thing, property);
            }
        }

        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        private Lazy<IThing?> _proxy;
        private string? _thingInterface;
        private readonly IThingManager _thingManager;
        private readonly ITypeManager _typeManager;
        private readonly ILogger<DynamicExtensionThing> _logger;

        [Configuration]
        public string? Property { get; set; }

        [Configuration]
        public string? ThingTitle { get; set; }

        [Configuration]
        public string? ThingInterface
        {
            get => _thingInterface; 
            set
            {
                _thingInterface = value;
                ResetProxy();
            }
        }

        [Configuration]
        public List<PropertyVariable> Variables { get; set; } = new List<PropertyVariable>();

        [Configuration]
        public List<Property> Properties { get; set; } = new List<Property>();

        [DynamicState]
        public IThing? Thing => _proxy.Value;

        public DynamicExtensionThing(IThingManager thingManager, ITypeManager typeManager,
            IEventManager eventManager, ILogger<DynamicExtensionThing> logger)
            : base(thingManager, eventManager)
        {
            _thingManager = thingManager;
            _typeManager = typeManager;
            _logger = logger;

            ResetProxy();
        }

        [MemberNotNull(nameof(_proxy))]
        private void ResetProxy()
        {
            _proxy = new Lazy<IThing?>(() =>
            {
                var interfaceType = _typeManager
                   .ThingInterfaces
                   .SingleOrDefault(t => t.FullName == ThingInterface);

                if (interfaceType != null)
                {
                    var proxy = (IThing)new ProxyGenerator()
                        .CreateInterfaceProxyWithoutTarget(interfaceType, this);

                    return proxy;
                }

                return null;
            });
        }

        protected override Task HandleMessageAsync(IEvent @event, CancellationToken cancellationToken)
        {
            if (@event is ThingStateChangedEvent stateChangedEvent)
            {
                if (Variables.Any(v => v.ThingId == stateChangedEvent.Thing.Id &&
                                       v.Property == stateChangedEvent.PropertyName))
                {
                    foreach (var variable in Variables
                        .Where(v => v.ThingId == stateChangedEvent.Thing.Id &&
                                    v.Property == stateChangedEvent.PropertyName))
                    {
                        variable.Apply(stateChangedEvent);
                    }

                    _thingManager.DetectChanges(this);
                }
            }

            if (!_proxy.IsValueCreated)
            {
                _thingManager.DetectChanges(this);
            }

            return Task.CompletedTask;
        }

        public void Intercept(IInvocation invocation)
        {
            try
            {
                if (invocation.Method?.Name == "get_Id")
                {
                    invocation.ReturnValue = Id + ".child";
                }
                else if (invocation.Method?.Name == "get_Title")
                {
                    invocation.ReturnValue = ThingTitle;
                }
                else if (invocation.Method?.Name != null &&
                    Properties.FirstOrDefault(p => p.Name == invocation.Method?.Name.Replace("get_", "")) is Property property)
                {
                    var engine = new Engine();

                    foreach (var variable in Variables)
                    {
                        var value = variable.TryGetValue(_thingManager);
                        engine.SetValue(variable.ActualName, value!);
                    }

                    try
                    {
                        var jsResult = engine
                            .Evaluate(property.Expression!)
                            .ToObject();

                        var json = JsonSerializer.Serialize(jsResult, _serializerOptions);
                        var result = JsonSerializer.Deserialize(json, invocation.Method.ReturnType, _serializerOptions);

                        invocation.ReturnValue = result;
                    }
                    catch
                    {
                        invocation.ReturnValue = null;
                    }
                }
                else
                {
                    invocation.Proceed();
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to intercept property.");
            }
        }
    }
}
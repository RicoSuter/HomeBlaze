using Castle.DynamicProxy;
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
using System.Diagnostics.CodeAnalysis;
using HomeBlaze.Services.Abstractions;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Messages;
using System.ComponentModel;
using System.Reflection;
using HomeBlaze.Components.Editors;

namespace HomeBlaze.Dynamic
{
    [DisplayName("Dynamic Thing")]
    [Description("Dynamically implement a Thing Interface and its properties.")]
    [ThingSetup(typeof(DynamicThingSetup), CanEdit = true, CanClone = true)]
    public class DynamicThing : ExtensionThing, IIconProvider, IInterceptor
    {
        public class DynamicStateAttribute : StateAttribute
        {
            public override string GetPropertyName(object thing, PropertyInfo property)
            {
                return ((DynamicThing)thing).PropertyName ?? base.GetPropertyName(thing, property);
            }
        }

        private readonly IThingManager _thingManager;
        private readonly ITypeManager _typeManager;
        private readonly ILogger<DynamicThing> _logger;

        private Lazy<Type?> _interfaceType;
        private Lazy<IThing?> _proxy;
        private string? _thingInterfaceName;

        public string IconName => "fa-solid fa-wand-magic-sparkles";

        [Configuration]
        public string? PropertyName { get; set; }

        [Configuration]
        public string? ThingTitle { get; set; }

        [Configuration]
        public string? ThingInterfaceName
        {
            get => _thingInterfaceName;
            set
            {
                _thingInterfaceName = value;
                ResetProxy();
            }
        }

        [Configuration]
        public List<PropertyVariable> Variables { get; set; } = new List<PropertyVariable>();

        [Configuration]
        public List<DynamicProperty> Properties { get; set; } = new List<DynamicProperty>();

        [DynamicState]
        public IThing? Thing => _proxy.Value;

        public DynamicThing(IThingManager thingManager, ITypeManager typeManager,
            IEventManager eventManager, ILogger<DynamicThing> logger)
            : base(thingManager, eventManager)
        {
            _thingManager = thingManager;
            _typeManager = typeManager;
            _logger = logger;

            ResetProxy();
        }

        [MemberNotNull(nameof(_interfaceType))]
        [MemberNotNull(nameof(_proxy))]
        private void ResetProxy()
        {
            _interfaceType = new Lazy<Type?>(() =>
            {
                return _typeManager
                   .ThingInterfaces
                   .SingleOrDefault(t => t.FullName == ThingInterfaceName);
            });

            _proxy = new Lazy<IThing?>(() =>
            {
                var interfaceType = _interfaceType.Value;
                if (interfaceType != null)
                {
                    var proxy = (IThing)new ProxyGenerator()
                        .CreateInterfaceProxyWithoutTarget(typeof(IThing), [interfaceType], new ProxyGenerationOptions
                        {
                            AdditionalAttributes =
                            {
                                CustomAttributeInfo.FromExpression(() => new ThingSetupAttribute(typeof(DynamicThingSetup))
                                {
                                    CanEdit = true,
                                    CanClone = true,
                                    EditParentThing = true,
                                })
                            }
                        }, this);

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
                                       v.PropertyName == stateChangedEvent.PropertyName))
                {
                    foreach (var variable in Variables
                        .Where(v => v.ThingId == stateChangedEvent.Thing.Id &&
                                    v.PropertyName == stateChangedEvent.PropertyName))
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
                    invocation.ReturnValue = Id + "/child";
                }
                else if (invocation.Method?.Name == "get_Title")
                {
                    invocation.ReturnValue = ThingTitle;
                }
                else if (invocation.Method?.Name != null &&
                    Properties.FirstOrDefault(p => p.Name == invocation.Method?.Name.Replace("get_", "")) is DynamicProperty property)
                {
                    invocation.ReturnValue = property.Evaluate(Variables, invocation.Method.ReturnType, _thingManager, _logger);
                }
                else
                {
                    var interfaceType = _interfaceType.Value;
                    if (interfaceType != null &&
                        invocation.Method?.Name != null)
                    {
                        // try to call default interface method
                        var method = interfaceType
                            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                            .Union(interfaceType
                                .GetInterfaces()
                                .SelectMany(i => i.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)))
                            .FirstOrDefault(m => m.Name.Split('.').Last() == invocation.Method.Name);

                        if (method != null && method.IsFinal)
                        {
                            try
                            {
                                invocation.ReturnValue = method
                                    .Invoke(invocation.Proxy, invocation.Arguments);
                            }
                            catch (Exception e2)
                            {
                                _logger.LogWarning(e2, "Failed to intercept property with default implementation.");
                            }
                        }
                        else
                        {
                            ProceedWithDefault(invocation);
                        }
                    }
                    else
                    {
                        ProceedWithDefault(invocation);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to intercept property.");
            }
        }

        private static void ProceedWithDefault(IInvocation invocation)
        {
            invocation.ReturnValue = invocation.Method.ReturnType?.IsValueType == true ?
                Activator.CreateInstance(invocation.Method.ReturnType) : null;
        }
    }
}
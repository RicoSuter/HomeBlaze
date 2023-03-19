using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System;

namespace HomeBlaze.Things
{
    public class SystemThing : GroupBase, IGroupThing, IIconProvider
    {
        public string? Id => "system." + InternalId;

        public string Title => "System";

        public string IconName => "fab fa-hubspot";

        [Configuration("id", IsIdentifier = true)]
        public string InternalId { get; set; } = Guid.NewGuid().ToString();

        [State]
        public PluginManager PluginManager { get; }

        [State]
        public SystemDiagnostics SystemDiagnostics { get; }

        public SystemThing(IThingManager thingManager, ITypeManager typeManager, 
            IEventManager eventManager, ILogger<PollingThing> logger)
            : base(thingManager)
        {
            SystemDiagnostics = new SystemDiagnostics(this, thingManager, eventManager, logger);
            PluginManager = new PluginManager(this, typeManager);
        }
    }
}
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace HomeBlaze.Things
{
    public class SystemThing : IRootThing, IIconProvider
    {
        public string? Id => "system." + InternalId;

        public string Title => "System";

        public string IconName => "fab fa-hubspot";

        [Configuration(IsIdentifier = true)]
        public string InternalId { get; set; } = Guid.NewGuid().ToString();

        [Configuration, State]
        public ICollection<IThing> Things { get; set; } = new List<IThing>();

        [State]
        public SystemDiagnostics SystemDiagnostics { get; }

        [State]
        public PluginManager PluginManager { get; }

        public SystemThing(IThingManager thingManager, ITypeManager typeManager, IEventManager eventManager, ILogger<PollingThing> logger)
        {
            SystemDiagnostics = new SystemDiagnostics(this, thingManager, eventManager, logger);
            PluginManager = new PluginManager(this, typeManager);
        }
    }
}
﻿using HomeBlaze.Abstractions;
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
        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Title => "System";

        public string IconName => "fab fa-hubspot";

        [State]
        public PluginManager PluginManager { get; }

        [State]
        public SystemDiagnostics SystemDiagnostics { get; }

        //[State]
        //public ThingsPage ThingsPage { get; } = new ThingsPage();

        public SystemThing(IThingManager thingManager, ITypeManager typeManager,
            IEventManager eventManager, ILogger<PollingThing> logger)
            : base(thingManager)
        {
            SystemDiagnostics = new SystemDiagnostics(this, eventManager, logger);
            PluginManager = new PluginManager(this, typeManager);
        }
    }
}
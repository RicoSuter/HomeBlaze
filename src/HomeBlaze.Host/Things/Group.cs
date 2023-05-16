using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Host.Things;
using System;

namespace HomeBlaze.Things
{
    [ThingSetup(typeof(GroupSetup), CanEdit = true)]
    public class Group : GroupBase, IThing, IGroupThing, IIconProvider
    {
        public string IconName => "fas fa-layer-group";

        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Configuration]
        public string? Title { get; set; }

        public Group(IThingManager thingManager) : base(thingManager)
        {
        }
    }
}
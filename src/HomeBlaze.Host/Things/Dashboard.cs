using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Host.Things;
using System;
using System.Collections.Generic;

namespace HomeBlaze.Things
{
    [ThingSetup(typeof(DashboardSetup), CanEdit = true)]
    public class Dashboard : IThing, IIconProvider
    {
        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title => "Dashboard (" + Name + ")";

        public string IconName => "fa-solid fa-table-columns";

        [Configuration]
        public string? Name { get; set; }

        [Configuration]
        public string? Icon { get; set; }

        [Configuration]
        public bool UseAutoScale { get; set; }

        [Configuration]
        public decimal Width { get; set; } = 800;

        [Configuration]
        public decimal Height { get; set; } = 480;

        [Configuration, State]
        public IList<Widget> Widgets { get; set; } = new List<Widget>();
    }
}
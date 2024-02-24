using HomeBlaze.Abstractions.Presentation;
using System;

namespace HomeBlaze.Things
{
    public class ThingsPage : IPageProvider
    {
        public string Id => "HomeBlaze.ThingsPage";

        public string? Title => "Things Page";

        public string? PageTitle => "Things";

        public Type? PageComponentType => typeof(Host.Pages.ThingsPage);
    }
}
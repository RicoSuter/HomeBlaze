using HomeBlaze.Abstractions;
using Microsoft.AspNetCore.Components;
using System;

namespace HomeBlaze.Host
{
    public static class NavigationManagerExtensions
    {
        public static void NavigateToThing(this NavigationManager navigationManager, IThing? thing)
        {
            navigationManager.NavigateTo("things?" +
                "thingId=" + Uri.EscapeDataString(thing?.Id ?? ""));
        }

        public static void NavigateToThingProperty(this NavigationManager navigationManager, IThing? thing, string propertyName)
        {
            navigationManager.NavigateTo("things?" +
                "thingId=" + Uri.EscapeDataString(thing?.Id ?? "") + "&" +
                "propertyName=" + propertyName);
        }

        public static void NavigateToCreateThing(this NavigationManager navigationManager, IThing? parentThing, IThing? extendedThing = null)
        {
            navigationManager.NavigateTo("things/new" +
                "?parentThingId=" + Uri.EscapeDataString(parentThing?.Id ?? "") +
                "&extendedThingId=" + Uri.EscapeDataString(extendedThing?.Id ?? ""));

        }

        public static void NavigateToEditThing(this NavigationManager navigationManager, IThing thing)
        {
            navigationManager.NavigateTo("things/" + thing.Id);
        }
    }
}

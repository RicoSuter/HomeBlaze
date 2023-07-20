using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Services;

namespace HomeBlaze.Services.Extensions
{
    public static class ThingExtensions
    {
        public static string? GetActualTitle(this IThing thing, IThingManager thingManager)
        {
            return thingManager
                .GetState(thing.Id, true)
                .OrderBy(s => s.Value.Attribute?.Order ?? 0)
                .GroupBy(p => p.Value.SourceThing)
                .SelectMany(g => g)
                .FirstOrDefault(g => g.Key == "Title")
                .Value.Value as string ?? thing.Title;
        }
    }
}

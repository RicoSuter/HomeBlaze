namespace HomeBlaze.Abstractions.Services
{
    public interface IThingManager
    {
        IGroupThing? RootThing { get; }

        IEnumerable<IThing> AllThings { get; }

        void DetectChanges(IThing thing);

        IThingMetadata? TryGetMetadata(IThing thing);

        IThing? TryGetById(string? thingId);

        IThing? TryGetParent(IThing thing);

        IReadOnlyDictionary<string, PropertyState> GetState(string? thingId, bool includeExtensions);

        PropertyState? TryGetPropertyState(string? thingId, string? property, bool includeExtensions);

        ThingOperation[] GetOperations(string? thingId, bool includeExtensions);

        IThing[] GetExtensionThings(IThing? thing);

        Task WriteConfigurationAsync(CancellationToken cancellationToken);
    }
}
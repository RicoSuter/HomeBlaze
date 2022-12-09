namespace HomeBlaze.Abstractions.Services
{
    public interface IThingManager
    {
        IThing? RootThing { get; }

        IEnumerable<IThing> AllThings { get; }

        void DetectChanges(IThing thing);

        IThingMetadata? TryGetMetadata(IThing thing);

        IThing? TryGetById(string? thingId);

        IThing? TryGetParent(IThing thing);

        IReadOnlyDictionary<string, PropertyState> GetState(IThing thing, bool includeExtensions);

        PropertyState? TryGetPropertyState(string thingId, string property, bool includeExtensions);

        ThingOperation[] GetOperations(IThing thing, bool includeExtensions);

        Task WriteConfigurationAsync(CancellationToken cancellationToken);
    }
}
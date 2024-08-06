namespace HomeBlaze.Abstractions.Services
{
    public class NullThingManager : IThingManager
    {
        public static NullThingManager Instance { get; private set; } = new NullThingManager();

        public IGroupThing? RootThing => null;

        public IEnumerable<IThing> AllThings => [];

        public void DetectChanges(IThing thing)
        {
        }

        protected NullThingManager()
        {

        }

        public IThing[] GetExtensionThings(IThing? thing)
        {
            return [];
        }

        public ThingOperation[] GetOperations(string? thingId, bool includeExtensions)
        {
            return [];
        }

        public IReadOnlyDictionary<string, PropertyState> GetState(string? thingId, bool includeExtensions)
        {
            return new Dictionary<string, PropertyState>();
        }

        public IThing? TryGetById(string? thingId)
        {
            return null;
        }

        public IThingMetadata? TryGetMetadata(IThing thing)
        {
            return null;
        }

        public IThing? TryGetParent(IThing thing)
        {
            return null;
        }

        public PropertyState? TryGetPropertyState(string? thingId, string? property, bool includeExtensions)
        {
            return null;
        }

        public Task WriteConfigurationAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
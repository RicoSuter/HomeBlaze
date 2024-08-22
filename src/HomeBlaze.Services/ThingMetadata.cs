using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Services
{
    public class ThingMetadata : IThingMetadata
    {
        public bool CanDelete => ThingSetupAttribute != null;

        public bool CanEdit => ThingSetupAttribute?.CanEdit == true;

        public bool CanClone => ThingSetupAttribute?.CanClone == true;

        public IReadOnlyDictionary<string, PropertyState>? CurrentFullState { get; set; }

        public IList<IThing> Children { get; } = [];

        IEnumerable<IThing> IThingMetadata.Children => Children;

        internal ThingSetupAttribute? ThingSetupAttribute { get; set; }

        internal IDisposable[] Disposables { get; set; } = [];
    }
}
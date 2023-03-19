using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Services;

namespace HomeBlaze.Services.Tests
{
    public class MockThingStorage : IThingStorage
    {
        private readonly IGroupThing _rootThing;

        public MockThingStorage(IGroupThing rootThing)
        {
            _rootThing = rootThing;
        }

        public Task<IGroupThing> ReadRootThingAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_rootThing);
        }

        public Task WriteRootThingAsync(IGroupThing thing, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}

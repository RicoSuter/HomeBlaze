using HomeBlaze.Messages;

using Namotion.Devices.Abstractions.Messages;

using Microsoft.Extensions.Logging.Abstractions;

namespace HomeBlaze.Services.Tests
{
    public class ThingManagerTests : IAsyncLifetime, IAsyncDisposable
    {
#pragma warning disable CS8618

        private MockRootThing _rootThing;
        private MockThingStorage _thingStorage;
        private TypeManager _typeManager;
        private EventManager _eventManager;
        private ThingManager _thingManager;
        private List<IEvent> _events;
        private IDisposable _eventsSubscription;

#pragma warning restore CS8618

        [Fact]
        public void WhenDetectChangesNotRun_ThenAllThingsIsEmpty()
        {
            Assert.Empty(_thingManager.AllThings);
        }

        [Fact]
        public void WhenDetectChangesIsRun_ThenRootThingIsInAllThings()
        {
            Test.Wait(() =>
            {
                Assert.Contains(_rootThing, _thingManager.AllThings);
            });
        }

        [Fact]
        public void WhenThingIsAdded_ThenItIsRegistered()
        {
            // Act
            var mockThing = new MockThing();
            _rootThing.AddThing(mockThing);

            Assert.Empty(_thingManager.AllThings);

            _thingManager.DetectChanges(_rootThing);

            // Assert
            Test.Wait(() =>
            {
                Assert.Contains(mockThing, _thingManager.AllThings);

                Assert.Contains(
                    _events.OfType<ThingRegisteredEvent>(),
                    e => e.Source == mockThing);

                Assert.Empty(_events.OfType<ThingUnregisteredEvent>());
            });
        }

        [Fact]
        public void WhenMovingUpThing_ThenItIsNotUnregistered()
        {
            // Arrange
            var mockThing1 = new MockThing();
            var mockThing2 = new MockThing();

            _rootThing.AddThing(mockThing1);
            _rootThing.AddThing(mockThing2);

            _thingManager.DetectChanges(_rootThing);

            Test.Wait(() =>
            {
                Assert.Equal(3, _thingManager.AllThings.Count());
            });

            // Act
            _rootThing.MoveThingUp(mockThing2);

            _thingManager.DetectChanges(_rootThing);
            Thread.Sleep(500);

            // Assert
            Test.Wait(() =>
            {
                Assert.Equal(3, _thingManager.AllThings.Count());
                Assert.Empty(_events.OfType<ThingUnregisteredEvent>());
            });
        }

        [Fact]
        public void WhenMovingDownThing_ThenItIsNotUnregistered()
        {
            // Arrange
            var mockThing1 = new MockThing();
            var mockThing2 = new MockThing();

            _rootThing.AddThing(mockThing1);
            _rootThing.AddThing(mockThing2);

            _thingManager.DetectChanges(_rootThing);

            Test.Wait(() =>
            {
                Assert.Equal(3, _thingManager.AllThings.Count());
            });

            // Act
            _rootThing.MoveThingDown(mockThing1);

            _thingManager.DetectChanges(_rootThing);
            Thread.Sleep(500);

            // Assert
            Test.Wait(() =>
            {
                Assert.Equal(3, _thingManager.AllThings.Count());
                Assert.Empty(_events.OfType<ThingUnregisteredEvent>());
            });
        }

        [Fact]
        public void WhenThingRemoved_ThenItIsUnregistered()
        {
            // Arrange
            var mockThing1 = new MockThing();
            var mockThing2 = new MockThing();

            _rootThing.AddThing(mockThing1);
            _rootThing.AddThing(mockThing2);

            _thingManager.DetectChanges(_rootThing);

            Test.Wait(() =>
            {
                Assert.Equal(3, _thingManager.AllThings.Count());
            });

            // Act
            _rootThing.RemoveThing(mockThing1);
            _thingManager.DetectChanges(_rootThing);

            // Assert
            Test.Wait(() =>
            {
                Assert.Equal(2, _thingManager.AllThings.Count());

                Assert.Single(_events
                    .OfType<ThingUnregisteredEvent>());

                Assert.Single(_events
                    .OfType<ThingUnregisteredEvent>()
                    .Where(e => e.Source == mockThing1));
            });
        }

        [Fact]
        public void WhenStateIsChanged_ThenStateChangedEventIsPublished()
        {
            // Arrange
            var mockThing = new MockThing();
            var expectedValue = 10;

            _rootThing.AddThing(mockThing);
            _thingManager.DetectChanges(_rootThing);

            Test.Wait(() =>
            {
                Assert.Equal(2, _thingManager.AllThings.Count());
            });

            // Act
            mockThing.Value = expectedValue;

            //var valueBeforeDetectChanges = _thingManager
            //    .TryGetPropertyState(mockThing.Id, nameof(mockThing.Value), true)?
            //    .Value;

            //Assert.Null(valueBeforeDetectChanges);

            _thingManager.DetectChanges(mockThing);

            // Assert
            Test.Wait(() =>
            {
                Assert.Single(_events
                    .OfType<ThingStateChangedEvent>()
                    .Where(e => e.Source == mockThing && Equals(e.NewValue, expectedValue)));
            });

            var valueAfterDetectChanges = _thingManager
                .TryGetPropertyState(mockThing.Id, nameof(mockThing.Value), true)?
                .Value;

            Assert.Equal(expectedValue, valueAfterDetectChanges);
        }

        public async Task InitializeAsync()
        {
            _rootThing = new MockRootThing();
            _thingStorage = new MockThingStorage(_rootThing);
            _typeManager = new TypeManager(null!, null!, NullLogger<TypeManager>.Instance);
            _eventManager = new EventManager();
            _thingManager = new ThingManager(_thingStorage, _typeManager, _eventManager, NullLogger<ThingManager>.Instance);

            _events = new List<IEvent>();
            _eventsSubscription = _eventManager.Subscribe(_events.Add);

            await _thingManager.StartAsync(CancellationToken.None);
            await _eventManager.StartAsync(CancellationToken.None);
        }

        public async Task DisposeAsync()
        {
            await _thingManager.StopAsync(CancellationToken.None);
            await _eventManager.StopAsync(CancellationToken.None);

            _eventManager.Dispose();
            _eventsSubscription.Dispose();
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await DisposeAsync();
        }
    }
}

using Namotion.Trackable.Model;
using Namotion.Trackable.Sources;
using System.Reactive.Subjects;

namespace Namotion.Trackable.Tests.Sources
{
    public class CompositeTrackableContextSourceTests
    {
        public class TestTrackableContextSource : ITrackableSource
        {
            public Dictionary<string, object?> Data { get; } = new Dictionary<string, object?>();

            public Action<PropertyInfo>? PropertyUpdateAction { get; private set; }

            public Task<IDisposable?> InitializeAsync(IEnumerable<PropertyInfo> properties, Action<PropertyInfo> propertyUpdateAction, CancellationToken cancellationToken)
            {
                PropertyUpdateAction = propertyUpdateAction;
                return Task.FromResult<IDisposable?>(new DummyDisposable());
            }

            public Task<IEnumerable<PropertyInfo>> ReadAsync(
                IEnumerable<PropertyInfo> properties, CancellationToken cancellationToken)
            {
                return Task.FromResult<IEnumerable<PropertyInfo>>(Data
                    .Where(p => properties.Any(u => u.Path == p.Key))
                    .Select(p => new PropertyInfo(properties.Single(u => u.Path == p.Key).Property, p.Key, p.Value))
                    .ToList());
            }

            public string? TryGetSourcePath(TrackedProperty property)
            {
                return property.Path;
            }

            public Task WriteAsync(IEnumerable<PropertyInfo> propertyChanges, CancellationToken cancellationToken)
            {
                foreach (var change in propertyChanges)
                {
                    Data[change.Path] = change.Value;
                }

                return Task.CompletedTask;
            }

            public class DummyDisposable : IDisposable
            {
                public void Dispose()
                {
                }
            }
        }

        [Fact]
        public async Task WhenInnerSourceReportsChangeThenOuterPathIsExpanded()
        {
            // Arrange
            var source = new TestTrackableContextSource();
            var compositeSource = new CompositeTrackableContextSource(new Dictionary<string, ITrackableSource>
            {
                { "abc", source }
            });

            // Act
            var property = new TrackedProperty<int>("abc.def", 0, new Tracker(), new Subject<TrackedPropertyChange>());
         
            PropertyInfo? reportedChangedProperty = null;
            await compositeSource.InitializeAsync([new PropertyInfo(property, "abc.def", 0)], (p) => reportedChangedProperty = p, CancellationToken.None);

            // simulate change from inner source
            source.PropertyUpdateAction?.Invoke(new PropertyInfo(property, "def", 1));

            // Assert
            Assert.Equal(1, reportedChangedProperty!.Value.Value);
        }

        [Fact]
        public async Task WhenWritingPropertyThenInnerSourceShouldHaveInnerPath()
        {
            // Arrange
            var source = new TestTrackableContextSource();
            var compositeSource = new CompositeTrackableContextSource(new Dictionary<string, ITrackableSource>
            {
                { "abc", source }
            });

            // Act
            var property = new TrackedProperty<int>("abc.def", 0, new Tracker(), new Subject<TrackedPropertyChange>());
            await compositeSource.WriteAsync(
            [
                new PropertyInfo(property, "abc.def", 1)
            ], CancellationToken.None);

            // Assert
            Assert.True(source.Data.ContainsKey("def"));
        }

        [Fact]
        public async Task WhenReadingPropertyThenInnerPathIsConverted()
        {
            // Arrange
            var property = new TrackedProperty<int>("abc.def", 0, new Tracker(), new Subject<TrackedPropertyChange>());
            var source = new TestTrackableContextSource
            {
                Data =
                {
                    { "def", 5 }
                }
            };

            var compositeSource = new CompositeTrackableContextSource(new Dictionary<string, ITrackableSource>
            {
                { "abc", source }
            });

            // Act
            var result = await compositeSource.ReadAsync([new PropertyInfo(property, "abc.def", 5)], CancellationToken.None);

            // Assert
            Assert.Contains(result, p => p.Value == (object)5);
        }
    }
}

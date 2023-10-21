using Namotion.Trackable.Sourcing;

namespace Namotion.Trackable.Tests.Sourcing
{
    public class CompositeTrackableContextSourceTests
    {
        public class TestTrackableContextSource : ITrackableContextSource
        {
            public string Separator => ".";

            public Dictionary<string, object?> Data { get; } = new Dictionary<string, object?>();

            public Action<string, object?>? PropertyUpdateAction { get; private set; }

            public Task<IReadOnlyDictionary<string, object?>> ReadAsync(IEnumerable<string> sourcePaths, CancellationToken cancellationToken)
            {
                return Task.FromResult<IReadOnlyDictionary<string, object?>>(Data.Where(p => sourcePaths.Contains(p.Key)).ToDictionary(p => p.Key, p => p.Value));
            }

            public Task<IDisposable> SubscribeAsync(IEnumerable<string> sourcePaths, Action<string, object?> propertyUpdateAction, CancellationToken cancellationToken)
            {
                PropertyUpdateAction = propertyUpdateAction;
                return Task.FromResult<IDisposable>(new DummyDisposable());
            }

            public Task WriteAsync(IReadOnlyDictionary<string, object?> propertyChanges, CancellationToken cancellationToken)
            {
                foreach (var change in propertyChanges)
                {
                    Data[change.Key] = change.Value;
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
            var compositeSource = new CompositeTrackableContextSource(new Dictionary<string, ITrackableContextSource>
            {
                { "abc", source }
            });

            // Act
            var reportedChangedPath = "";
            await compositeSource.SubscribeAsync(new[] { "abc.def" }, (path, value) => reportedChangedPath = path, CancellationToken.None);
            
            // simulate change from inner source
            source.PropertyUpdateAction?.Invoke("def", null);

            // Assert
            Assert.Equal("abc.def", reportedChangedPath);
        }

        [Fact]
        public async Task WhenWritingPropertyThenInnerSourceShouldHaveInnerPath()
        {
            // Arrange
            var source = new TestTrackableContextSource();
            var compositeSource = new CompositeTrackableContextSource(new Dictionary<string, ITrackableContextSource>
            {
                { "abc", source }
            });

            // Act
            await compositeSource.WriteAsync(new Dictionary<string, object?>
            {
                { "abc.def", 1 }
            }, CancellationToken.None);

            // Assert
            Assert.True(source.Data.ContainsKey("def"));
        }

        [Fact]
        public async Task WhenReadingPropertyThenInnerPathIsConverted()
        {
            // Arrange
            var source = new TestTrackableContextSource
            {
                Data =
                {
                    { "def", 5 }
                }
            };

            var compositeSource = new CompositeTrackableContextSource(new Dictionary<string, ITrackableContextSource>
            {
                { "abc", source }
            });

            // Act
            var result = await compositeSource.ReadAsync(new[] { "abc.def" }, CancellationToken.None);

            // Assert
            Assert.Equal(5, result["abc.def"]);
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

using Namotion.Trackable;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.Sources;
using Namotion.Trackable.Validation;

namespace Namotion.Things.Tests;

public class TrackableContextWithSourceTests
{
    public class Car
    {
        [Trackable]
        [TrackableSourcePath("mqtt", "FLT")]
        public required virtual Tire FrontLeftTire { get; set; }

        [Trackable]
        [TrackableSourcePath("mqtt", "FRT")]
        public virtual Tire? FrontRightTire { get; set; }
    }

    public class Tire
    {
        [Trackable]
        [TrackableSource("mqtt", "pressure")]
        public virtual decimal Pressure { get; set; }
    }

    [Fact]
    public void ShouldFindSourcePaths()
    {
        // Arrange
        var trackableContext = CreateContext<Car>();
        var trackable = trackableContext.Object;

        // Act
        var result = trackableContext.AllProperties
            .Select(p => p.TryGetAttributeBasedSourcePath("mqtt", trackableContext))
            .Where(p => p != null)
            .ToArray();

        // Assert
        Assert.Single(result);
    }

    private static TrackableContext<T> CreateContext<T>()
        where T : class
    {
        var serviceCollection = new ServiceCollection();
        return new TrackableContext<T>(
            Array.Empty<ITrackablePropertyValidator>(),
            Array.Empty<ITrackableInterceptor>(),
            serviceCollection.BuildServiceProvider());
    }

}

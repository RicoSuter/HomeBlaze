using Microsoft.Extensions.DependencyInjection;

using Namotion.Trackable;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.Sourcing;
using Namotion.Trackable.Validation;

namespace Namotion.Things.Tests;

public class SourcingTrackableContextTests
{
    public class Car
    {
        [Trackable]
        [TrackableSource("mqtt", "FLT")]
        public required virtual Tire FrontLeftTire { get; set; }

        [Trackable]
        [TrackableSource("mqtt", "FRT")]
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
        Assert.Equal(2, result.Length);
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

using Microsoft.Extensions.DependencyInjection;

using Namotion.Trackable;
using Namotion.Trackable.Sourcing;
using Namotion.Trackable.Validation;
using System.ComponentModel.DataAnnotations;

namespace Namotion.Things.Tests;

public class SourcingTrackableContextTests
{
    public class Car
    {
        [TrackableFromSource(RelativePath = "FLT")]
        public required virtual Tire FrontLeftTire { get; set; }

        [TrackableFromSource(RelativePath = "FRT"), Required]
        public virtual Tire? FrontRightTire { get; set; }
    }

    public class Tire
    {
        [TrackableFromSource(RelativePath = "pressure")]
        public virtual decimal Pressure { get; set; }
    }

    [Fact]
    public void ShouldFindSourcePaths()
    {
        // Arrange
        var thingContext = CreateContext<Car>();
        var thing = thingContext.Object;

        // Act
        var result = thingContext.AllProperties
            .Select(p => p.TryGetSourcePath())
            .Where(p => p != null)
            .ToArray();

        // Assert
        Assert.Equal(4, result.Length);
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

using Microsoft.Extensions.DependencyInjection;

using Namotion.Trackable;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.Model;
using Namotion.Trackable.Sources;
using Namotion.Trackable.Validation;
using System.ComponentModel.DataAnnotations;

namespace Namotion.Things.Tests;

public class DictionaryTrackableContextTests
{
    public class Car
    {
        public Car(ITrackableFactory factory)
        {
            Tires = new Dictionary<string, Tire>
            {
                { "FL", factory.CreateProxy<Tire>() },
                { "FR", factory.CreateProxy<Tire>() },
                { "RL", factory.CreateProxy<Tire>() },
                { "RR", factory.CreateProxy<Tire>() }
            };
        }

        [Trackable]
        [TrackableSourcePath("mqtt", "tires")]
        public virtual IReadOnlyDictionary<string, Tire> Tires { get; set; }
    }

    public class Tire
    {
        [Trackable]
        [TrackableSource("mqtt", "pressure")]
        public virtual decimal Pressure { get; set; }
    }

    [Fact]
    public void ShouldTrackAllTrackablesInDictionary()
    {
        // Arrange
        var trackableContext = CreateContext<Car>();
        var trackable = trackableContext.Object;

        var changes = new List<TrackedPropertyChange>();
        trackableContext.Subscribe(changes.Add);

        // Act
        trackable.Tires["FL"].Pressure = 1;
        trackable.Tires["FR"].Pressure = 1;
        trackable.Tires["RL"].Pressure = 1;

        // Assert
        Assert.Equal(3, changes.Count);
    }

    [Fact]
    public void ShouldHaveCorrectDictionaryPaths()
    {
        // Arrange
        var trackableContext = CreateContext<Car>();
        var trackable = trackableContext.Object;

        // Act
        var firstTirePressure = trackableContext
            .AllProperties
            .First(v => v.PropertyName == nameof(Tire.Pressure));

        // Assert
        Assert.Equal("Tires[FL].Pressure", firstTirePressure.Path);
        Assert.Equal("tires[FL].pressure", firstTirePressure.TryGetAttributeBasedSourcePath("mqtt", trackableContext));
    }

    public class Garage
    {
        [Required]
        [Trackable]
        [TrackableSourcePath("mqtt", "car")]
        public virtual required Car Car { get; set; }
    }

    [Fact]
    public void ShouldHaveCorrectDeepPathsInDictionary()
    {
        // Arrange
        var trackableContext = CreateContext<Garage>();
        var trackable = trackableContext.Object;

        // Act
        var firstTirePressure = trackableContext
            .AllProperties
            .First(v => v.PropertyName == nameof(Tire.Pressure));

        // Assert
        Assert.Equal("Car.Tires[FL].Pressure", firstTirePressure.Path);
        Assert.Equal("car.tires[FL].pressure", firstTirePressure.TryGetAttributeBasedSourcePath("mqtt", trackableContext));
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

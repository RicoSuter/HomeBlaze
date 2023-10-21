using Microsoft.Extensions.DependencyInjection;

using Namotion.Trackable;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.Model;
using Namotion.Trackable.Sourcing;
using Namotion.Trackable.Validation;

namespace Namotion.Things.Tests;

public class CollectionTrackableContextTests
{
    public class Car
    {
        public Car(ITrackableFactory factory)
        {
            Tires = new Tire[]
            {
                factory.CreateProxy<Tire>(),
                factory.CreateProxy<Tire>(),
                factory.CreateProxy<Tire>(),
                factory.CreateProxy<Tire>() 
            };
        }

        [Trackable]
        [TrackableSource("mqtt", "tires")]
        public virtual Tire[] Tires { get; set; }
    }

    public class Tire
    {
        [Trackable]
        [TrackableSource("mqtt", "pressure")]
        public virtual decimal Pressure { get; set; }
    }

    [Fact]
    public void ShouldTrackAllTrackablesInArray()
    {
        // Arrange
        var trackableContext = CreateContext<Car>();
        var trackable = trackableContext.Object;
       
        var changes = new List<TrackedPropertyChange>();
        trackableContext.Subscribe(changes.Add);

        // Act
        trackable.Tires[0].Pressure = 1;
        trackable.Tires[1].Pressure = 1;
        trackable.Tires[2].Pressure = 1;

        // Assert
        Assert.Equal(3, changes.Count);
    }

    [Fact]
    public void ShouldHaveCorrectPaths()
    {
        // Arrange
        var trackableContext = CreateContext<Car>();
        var trackable = trackableContext.Object;

        // Act
        var firstTirePressure = trackableContext
            .AllProperties
            .First(v => v.PropertyName == nameof(Tire.Pressure));

        // Assert
        Assert.Equal("Tires[0].Pressure", firstTirePressure.Path);
        Assert.Equal("tires[0].pressure", firstTirePressure.TryGetSourcePath("mqtt"));
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

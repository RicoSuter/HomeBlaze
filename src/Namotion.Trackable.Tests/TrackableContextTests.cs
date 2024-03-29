using Microsoft.Extensions.DependencyInjection;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.Model;

using System.ComponentModel.DataAnnotations;

namespace Namotion.Trackable.Tests;

public class TrackableContextTests
{
    private TrackableFactory _factory;

    public TrackableContextTests()
    {
        var serviceCollection = new ServiceCollection();
        _factory = new TrackableFactory(serviceCollection.BuildServiceProvider());
    }

    public class Person
    {
        [Trackable]
        public virtual string? FirstName { get; set; }

        [Trackable]
        public virtual string? LastName { get; set; }

        [Trackable]
        public virtual string FullName => $"{FirstName} {LastName}";

        [Trackable]
        public virtual Person? Father { get; set; }
    }

    [Fact]
    public void ShouldRaiseChangedForPropertyAndDerivedProperty()
    {
        // Arrange
        var trackableContext = new TrackableContext<Person>(_factory);
        var trackable = trackableContext.Object;
        trackable.FirstName = "Rico";
        trackable.LastName = "Suter";

        var changes = new List<TrackedPropertyChange>();
        trackableContext.Subscribe(changes.Add);

        // Act
        trackable.LastName = "Doe";

        // Assert
        Assert.Equal(2, changes.Count);
        Assert.Equal(nameof(Person.LastName), changes[0].Property.Name);
        Assert.Equal(nameof(Person.FullName), changes[1].Property.Name);
    }

    [Fact]
    public void ShouldTrackChangesOfCreatedThing()
    {
        // Arrange
        var trackableContext = new TrackableContext<Person>(_factory);
        var trackable = trackableContext.Object;

        var father = _factory!.CreateProxy<Person>();
        trackable.Father = father;

        var changes = new List<TrackedPropertyChange>();
        trackableContext.Subscribe(changes.Add);

        // Act
        father.FirstName = "John";

        // Assert
        Assert.Equal(2, changes.Count); // firstname & fullname
        Assert.Equal("John", changes[0].Value);
    }

    [Fact]
    public void ShouldNotTrackChangesOfRemovedThing()
    {
        // Arrange
        var trackableContext = new TrackableContext<Person>(_factory);
        var trackable = trackableContext.Object;

        var father = _factory!.CreateProxy<Person>();
        trackable.Father = father;
        trackable.Father = null;

        var changes = new List<TrackedPropertyChange>();
        trackableContext.Subscribe(changes.Add);

        // Act
        father.FirstName = "John";

        // Assert
        Assert.Empty(changes);
    }

    public class Car
    {
        public Car(ITrackableFactory thingFactory)
        {
            FrontLeftTire = thingFactory.CreateProxy<Tire>();
        }

        [Trackable]
        public virtual Tire FrontLeftTire { get; set; }
    }

    public class Tire
    {
        [Trackable]
        public virtual decimal Pressure { get; set; }
    }

    [Fact]
    public void ShouldTrackChangesOfInternallyCreatedThing()
    {
        // Arrange
        var thingContext = new TrackableContext<Car>(_factory);
        var thing = thingContext.Object;

        var changes = new List<TrackedPropertyChange>();
        thingContext.Subscribe(changes.Add);

        // Act
        thing.FrontLeftTire.Pressure = 5;

        // Assert
        Assert.Single(changes);
        Assert.Equal(5m, changes[0].Value);
    }

    public class CarWithRequiredTires
    {
        [Trackable]
        public required virtual Tire FrontLeftTire { get; set; }

        [Trackable, Required]
        public virtual Tire? FrontRightTire { get; set; }
    }

    [Fact]
    public void ShouldAutoCreateRequiredProperties()
    {
        // Arrange
        var thingContext = new TrackableContext<CarWithRequiredTires>(_factory);
        var thing = thingContext.Object;

        var changes = new List<TrackedPropertyChange>();
        thingContext.Subscribe(changes.Add);

        // Act
        thing.FrontLeftTire.Pressure = 5; // thing.FrontLeftTire must not be null here
        thing.FrontRightTire!.Pressure = 10; // thing.FrontLeftTire must not be null here

        // Assert
        Assert.Equal(5m, changes[0].Value);
        Assert.Equal(10m, changes[1].Value);
    }

    [Fact]
    public void ShouldObserveProperty()
    {
        // Arrange
        var thingContext = new TrackableContext<CarWithRequiredTires>(_factory);
        var thing = thingContext.Object;

        var changes = new List<decimal>();
        thingContext
            .Observe(c => c.FrontLeftTire.Pressure)
            .Subscribe(changes.Add);

        // Act
        thing.FrontLeftTire.Pressure = 5;
        thing.FrontLeftTire!.Pressure = 10;
        thing.FrontRightTire!.Pressure = 3;

        // Assert
        Assert.Equal(5m, changes[0]);
        Assert.Equal(10m, changes[1]);
    }
}

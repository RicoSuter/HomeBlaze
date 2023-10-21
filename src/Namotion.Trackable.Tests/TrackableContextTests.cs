using Microsoft.Extensions.DependencyInjection;

using Namotion.Trackable;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.Model;
using Namotion.Trackable.Validation;
using System.ComponentModel.DataAnnotations;

namespace Namotion.Things.Tests;

public class TrackableContextTests
{
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
        var trackableContext = CreateContext<Person>();
        var trackable = trackableContext.Object;
        trackable.FirstName = "Rico";
        trackable.LastName = "Suter";

        var changes = new List<TrackedPropertyChange>();
        trackableContext.Subscribe(changes.Add);

        // Act
        trackable.LastName = "Doe";

        // Assert
        Assert.Equal(2, changes.Count);
        Assert.Equal(nameof(Person.LastName), changes[0].Property.PropertyName);
        Assert.Equal(nameof(Person.FullName), changes[1].Property.PropertyName);
    }

    [Fact]
    public void ShouldTrackChangesOfCreatedThing()
    {
        // Arrange
        var trackableContext = CreateContext<Person>();
        var trackable = trackableContext.Object;

        var father = trackableContext.CreateProxy<Person>();
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
        var trackableContext = CreateContext<Person>();
        var trackable = trackableContext.Object;

        var father = trackableContext.CreateProxy<Person>();
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
        var thingContext = CreateContext<Car>();
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
        var thingContext = CreateContext<CarWithRequiredTires>();
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

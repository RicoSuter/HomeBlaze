﻿using Microsoft.Extensions.DependencyInjection;

using Namotion.Trackable.Attributes;
using Namotion.Trackable.Model;
using Namotion.Trackable.Sources;

using System.ComponentModel.DataAnnotations;

namespace Namotion.Trackable.Tests;

public class CollectionTrackableContextTests
{
    private TrackableFactory _factory;

    public CollectionTrackableContextTests()
    {
        var serviceCollection = new ServiceCollection();
        _factory = new TrackableFactory(serviceCollection.BuildServiceProvider());
    }

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
        [TrackableSourcePath("mqtt", "tires")]
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
    public void ShouldHaveCorrectArrayPaths()
    {
        // Arrange
        var trackableContext = CreateContext<Car>();
        var trackable = trackableContext.Object;

        // Act
        var firstTirePressure = trackableContext
            .AllProperties
            .First(v => v.Name == nameof(Tire.Pressure));

        // Assert
        Assert.Equal("Tires[0].Pressure", firstTirePressure.Path);
        Assert.Equal("tires[0].pressure", firstTirePressure.TryGetAttributeBasedSourcePath("mqtt", trackableContext));
    }

    public class Garage
    {
        [Required]
        [Trackable]
        [TrackableSourcePath("mqtt", "car")]
        public virtual required Car Car { get; set; }
    }

    [Fact]
    public void ShouldHaveCorrectDeepPathsInArray()
    {
        // Arrange
        var trackableContext = CreateContext<Garage>();
        var trackable = trackableContext.Object;

        // Act
        var firstTirePressure = trackableContext
            .AllProperties
            .First(v => v.Name == nameof(Tire.Pressure));

        // Assert
        Assert.Equal("Car.Tires[0].Pressure", firstTirePressure.Path);
        Assert.Equal("car.tires[0].pressure", firstTirePressure.TryGetAttributeBasedSourcePath("mqtt", trackableContext));
    }

    [Fact]
    public void ShouldCorrectlyDetatchTrackers()
    {
        // Arrange
        var trackableContext = CreateContext<Garage>();
        var trackable = trackableContext.Object;

        // Act & Assert
        Assert.Equal(6, trackableContext.AllTrackers.Count);
        trackable.Car = _factory.CreateProxy<Car>();
        Assert.Equal(6, trackableContext.AllTrackers.Count);
    }

    private TrackableContext<T> CreateContext<T>()
        where T : class
    {
        return new TrackableContext<T>(_factory);
    }
}

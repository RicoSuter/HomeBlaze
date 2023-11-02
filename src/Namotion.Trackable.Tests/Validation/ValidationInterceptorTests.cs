using Microsoft.Extensions.DependencyInjection;

using Namotion.Trackable;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.Model;
using Namotion.Trackable.Validation;
using System.ComponentModel.DataAnnotations;

namespace Namotion.Things.Tests;

public class ValidationInterceptorTests
{
    public class Person
    {
        [Trackable, MaxLength(4)]
        public virtual string? FirstName { get; set; }

        [Trackable]
        public virtual string? LastName { get; set; }

        [Trackable]
        public virtual string FullName => $"{FirstName} {LastName}";

        [Trackable]
        public virtual Person? Father { get; set; }
    }

    [Fact]
    public void ShouldValidateProperty()
    {
        // Arrange
        var trackableContext = CreateContext<Person>(new DataAnnotationsValidator());
        var trackable = trackableContext.Object;

        // Act
        trackable.FirstName = "Rico"; // allowed

        // Assert
        Assert.Throws<ValidationException>(() =>
        {
            trackable.FirstName = "Suter"; // not allowed
        });
    }

    private static TrackableContext<T> CreateContext<T>(params ITrackablePropertyValidator[] propertyValidators)
        where T : class
    {
        var serviceCollection = new ServiceCollection();
        return new TrackableContext<T>(new TrackableFactory(
            new ITrackableInterceptor[] { new ValidationTrackableInterceptor(propertyValidators) },
            serviceCollection.BuildServiceProvider()));
    }
}

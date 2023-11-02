using Microsoft.Extensions.DependencyInjection;

using Namotion.Trackable;
using Namotion.Trackable.Attributes;
using Namotion.Trackable.Transaction;
using Namotion.Trackable.Validation;
using System.ComponentModel.DataAnnotations;

namespace Namotion.Things.Tests;

public class TransactionInterceptorTests
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
    public void ShouldRollbackChangedProperties()
    {
        // Arrange
        var trackableContext = CreateContext<Person>(new DataAnnotationsValidator());
        var trackable = trackableContext.Object;

        // Act
        trackable.FirstName = "Rico";
        trackable.LastName = "Suter";

        // Assert

    }

    private static TrackableContext<T> CreateContext<T>(params ITrackablePropertyValidator[] propertyValidators)
        where T : class
    {
        var serviceCollection = new ServiceCollection();
        return new TrackableContext<T>(new TrackableFactory(
            new ITrackableInterceptor[] { new TransactionTrackableInterceptor() },
            serviceCollection.BuildServiceProvider()));
    }
}

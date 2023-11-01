using Microsoft.Extensions.DependencyInjection;

using Namotion.Trackable;
using Namotion.Trackable.Attributes;

namespace Namotion.Things.Tests;

public class TrackableInterceptorTests
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
    public void WhenTrackableIsRemoved_ThenContextIsRemoved()
    {
        // Arrange
        var trackableContext = CreateContext<Person>();
        var trackable = trackableContext.Object;

        var father = trackableContext.CreateProxy<Person>();
        var fatherInterceptor = ((ITrackable)father).Interceptor;

        // Act & Assert
        Assert.DoesNotContain(trackableContext, fatherInterceptor.Contexts);

        trackable.Father = father;
        Assert.Contains(trackableContext, fatherInterceptor.Contexts);

        trackable.Father = null;
        Assert.DoesNotContain(trackableContext, fatherInterceptor.Contexts);
    }

    private static TrackableContext<T> CreateContext<T>()
        where T : class
    {
        var serviceCollection = new ServiceCollection();
        return new TrackableContext<T>(
            Array.Empty<ITrackableInterceptor>(),
            serviceCollection.BuildServiceProvider());
    }
}

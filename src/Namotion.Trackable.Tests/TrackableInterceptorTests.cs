using Microsoft.Extensions.DependencyInjection;
using Namotion.Trackable.Attributes;

namespace Namotion.Trackable.Tests;

public class TrackableInterceptorTests
{
    private static TrackableFactory? _factory;

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

        var father = _factory!.CreateProxy<Person>();
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
        _factory = new TrackableFactory(serviceCollection.BuildServiceProvider());
        return new TrackableContext<T>(_factory);
    }
}

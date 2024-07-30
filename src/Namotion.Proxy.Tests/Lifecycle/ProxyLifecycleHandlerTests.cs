using Namotion.Proxy.Abstractions;

namespace Namotion.Proxy.Tests.Lifecycle;

public class ProxyLifecycleHandlerTests
{
    [Fact]
    public void WhenAssigningArray_ThenAllProxiesAreAttached()
    {
        // Arrange
        var attaches = new List<ProxyLifecycleContext>();
        var detaches = new List<ProxyLifecycleContext>();

        var handler = new TestProxyPropertyRegistryHandler(attaches, detaches);
        var context = ProxyContext
            .CreateBuilder()
            .WithProxyLifecycle()
            .AddHandler(handler)
            .Build();

        // Act
        var mother = new Person(context) { FirstName = "Mother" };
        var child2 = new Person { FirstName = "Child1" };
        var child3 = new Person { FirstName = "Child2" };

        mother.Children = [child2, child3];
        mother.Children = [child2];

        // Assert
        Assert.Equal(3, attaches.Count);
        Assert.Single(detaches);
    }

    [Fact]
    public void WhenCallingSetContext_ThenArrayItemsAreAttached()
    {
        // Arrange
        var attaches = new List<ProxyLifecycleContext>();
        var detaches = new List<ProxyLifecycleContext>();

        var handler = new TestProxyPropertyRegistryHandler(attaches, detaches);
        var context = ProxyContext
            .CreateBuilder()
            .WithProxyLifecycle()
            .AddHandler(handler)
            .Build();

        // Act
        var mother = new Person { FirstName = "Mother" };
        var child2 = new Person { FirstName = "Child1" };
        var child3 = new Person { FirstName = "Child2" };

        mother.Children = [child2, child3];
        mother.SetContext(context);

        // Assert
        Assert.Equal(3, attaches.Count);
    }

    [Fact]
    public void WhenAssigningProxy_ThenAllProxyIsAttached()
    {
        // Arrange
        var attaches = new List<ProxyLifecycleContext>();
        var detaches = new List<ProxyLifecycleContext>();

        var handler = new TestProxyPropertyRegistryHandler(attaches, detaches);
        var context = ProxyContext
            .CreateBuilder()
            .WithAutomaticContextAssignment()
            .WithProxyLifecycle()
            .AddHandler(handler)
            .Build();

        // Act
        var mother1 = new Person(context) { FirstName = "Mother1" };
        var mother2 = new Person { FirstName = "Mother2" };
        var mother3 = new Person { FirstName = "Mother3" };

        mother1.Mother = mother2;
        mother2.Mother = mother3;

        mother1.Mother = null;

        // Assert
        Assert.Equal(3, attaches.Count);
        Assert.Equal(2, detaches.Count);
    }

    [Fact]
    public void WhenCallingSetContext_ThenAllChildrenAreAlsoAttached()
    {
        // Arrange
        var attaches = new List<ProxyLifecycleContext>();
        var detaches = new List<ProxyLifecycleContext>();

        var handler = new TestProxyPropertyRegistryHandler(attaches, detaches);
        var context = ProxyContext
            .CreateBuilder()
            .WithAutomaticContextAssignment()
            .WithProxyLifecycle()
            .AddHandler(handler)
            .Build();

        // Act
        var mother1 = new Person { FirstName = "Mother1" };
        var mother2 = new Person { FirstName = "Mother2" };
        var mother3 = new Person { FirstName = "Mother3" };

        mother1.Mother = mother2;
        mother2.Mother = mother3;

        mother1.SetContext(context);

        // Assert
        Assert.Equal(3, attaches.Count);
    }

    [Fact]
    public void WhenCallingSetContextWithNull_ThenArrayItemsAreNotDetached()
    {
        // Arrange
        var attaches = new List<ProxyLifecycleContext>();
        var detaches = new List<ProxyLifecycleContext>();

        var handler = new TestProxyPropertyRegistryHandler(attaches, detaches);
        var context = ProxyContext
            .CreateBuilder()
            .WithProxyLifecycle()
            .AddHandler(handler)
            .Build();

        // Act
        var mother = new Person(context) { FirstName = "Mother" };
        var child2 = new Person { FirstName = "Child1" };
        var child3 = new Person { FirstName = "Child2" };

        mother.Children = [child2, child3];
        mother.SetContext(null);

        // Assert
        Assert.Single(detaches);
    }

    [Fact]
    public void WhenCallingSetContextWithNull_ThenChildrenAreNotDetached()
    {
        // Arrange
        var attaches = new List<ProxyLifecycleContext>();
        var detaches = new List<ProxyLifecycleContext>();

        var handler = new TestProxyPropertyRegistryHandler(attaches, detaches);
        var context = ProxyContext
            .CreateBuilder()
            .WithAutomaticContextAssignment()
            .WithProxyLifecycle()
            .AddHandler(handler)
            .Build();

        // Act
        var mother1 = new Person(context) { FirstName = "Mother1" };
        var mother2 = new Person { FirstName = "Mother2" };
        var mother3 = new Person { FirstName = "Mother3" };

        mother1.Mother = mother2;
        mother2.Mother = mother3;

        mother1.SetContext(null);

        // Assert
        Assert.Single(detaches);
    }
}

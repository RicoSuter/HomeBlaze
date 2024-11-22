using Namotion.Proxy.Abstractions;
using Namotion.Proxy.Registry;
using Namotion.Proxy.Registry.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Namotion.Proxy.Tests.Registry;

public class ProxyRegistryTests
{
    [Fact]
    public void WhenTwoChildrenAreAttachedSequentially_ThenWeHaveThreeAttaches()
    {
        // Arrange
        var attaches = new List<ProxyLifecycleContext>();
        var detaches = new List<ProxyLifecycleContext>();

        var handler = new TestProxyPropertyRegistryHandler(attaches, detaches);
        var context = ProxyContext
            .CreateBuilder()
            .WithRegistry()
            .AddHandler(handler)
            .Build();

        // Act
        var person = new Person(context)
        {
            FirstName = "Child",
            Mother = new Person
            {
                FirstName = "Susi",
                Mother = new Person
                {
                    FirstName = "Susi2"
                }
            }
        };

        // Assert
        Assert.Equal(3, attaches.Count);
        Assert.Empty(detaches);

        var registry = context.GetHandler<IProxyRegistry>();
        Assert.Equal(3, registry.KnownProxies.Count());
    }

    [Fact]
    public void WhenTwoChildrenAreAttachedInOneBranch_ThenWeHaveThreeAttaches()
    {
        // Arrange
        var attaches = new List<ProxyLifecycleContext>();
        var detaches = new List<ProxyLifecycleContext>();

        var handler = new TestProxyPropertyRegistryHandler(attaches, detaches);
        var context = ProxyContext
            .CreateBuilder()
            .WithRegistry()
            .AddHandler(handler)
            .Build();

        // Act
        var person = new Person(context)
        {
            FirstName = "Child"
        };

        person.Mother = new Person
        {
            FirstName = "Susi",
            Mother = new Person
            {
                FirstName = "Susi2"
            }
        };

        // Assert
        Assert.Equal(3, attaches.Count);
        Assert.Empty(detaches);

        var registry = context.GetHandler<IProxyRegistry>();
        Assert.Equal(3, registry.KnownProxies.Count());
    }

    [Fact]
    public void WhenProxyWithChildProxyIsRemoved_ThenWeHaveTwoDetaches()
    {
        // Arrange
        var attaches = new List<ProxyLifecycleContext>();
        var detaches = new List<ProxyLifecycleContext>();

        var handler = new TestProxyPropertyRegistryHandler(attaches, detaches);
        var context = ProxyContext
            .CreateBuilder()
            .WithRegistry()
            .AddHandler(handler)
            .Build();

        // Act
        var person = new Person(context)
        {
            FirstName = "Child",
            Mother = new Person
            {
                FirstName = "Susi",
                Mother = new Person
                {
                    FirstName = "Susi2"
                }
            }
        };

        person.Mother = null;

        // Assert
        Assert.Equal(3, attaches.Count);
        Assert.Equal(2, detaches.Count);

        var registry = context.GetHandler<IProxyRegistry>();
        Assert.Single(registry.KnownProxies);
    }

    [Fact]
    public void WhenAddingTransitiveProxies_ThenAllAreAvailable()
    {
        // Arrange
        var context = ProxyContext
            .CreateBuilder()
            .WithRegistry()
            .Build();

        var registry = context.GetHandler<IProxyRegistry>();

        // Act
        var grandmother = new Person
        {
            FirstName = "Grandmother"
        };

        var mother = new Person
        {
            FirstName = "Mother",
            Mother = grandmother
        };

        var person = new Person(context)
        {
            FirstName = "Child",
            Mother = mother
        };

        // Assert
        Assert.Equal(3, registry.KnownProxies.Count());
        Assert.Contains(person, registry.KnownProxies.Keys);
        Assert.Contains(mother, registry.KnownProxies.Keys);
        Assert.Contains(grandmother, registry.KnownProxies.Keys);
    }

    [Fact]
    public void WhenRemovingMiddleElement_ThenChildrensAreAlsoRemoved()
    {
        // Arrange
        var context = ProxyContext
            .CreateBuilder()
            .WithRegistry()
            .Build();

        var registry = context.GetHandler<IProxyRegistry>();

        // Act
        var grandmother = new Person
        {
            FirstName = "Grandmother"
        };

        var mother = new Person
        {
            FirstName = "Mother",
            Mother = grandmother
        };

        var person = new Person(context)
        {
            FirstName = "Child",
            Mother = mother
        };

        mother.Mother = null;

        // Assert
        Assert.Equal(2, registry.KnownProxies.Count());
        Assert.Contains(person, registry.KnownProxies.Keys);
        Assert.Contains(mother, registry.KnownProxies.Keys);
        Assert.DoesNotContain(grandmother, registry.KnownProxies.Keys);
    }

    [Fact]
    public async Task WhenConvertingToJson_ThenGraphIsPreserved()
    {
        // Arrange
        var context = ProxyContext
            .CreateBuilder()
            .WithRegistry()
            .Build();

        // Act
        var person = new Person(context)
        {
            FirstName = "Child",
            Mother = new Person
            {
                FirstName = "Susi",
                Mother = new Person
                {
                    FirstName = "Susi2"
                }
            }
        };

        // Assert
        await Verify(person.ToJsonObject().ToJsonString(new JsonSerializerOptions(JsonSerializerOptions.Default) { WriteIndented = true }));
    }
}

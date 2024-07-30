using Namotion.Proxy.Lifecycle;

namespace Namotion.Proxy.Tests.Lifecycle;

public class ParentsHandlerTests
{
    [Fact]
    public void WhenProxyIsReferencedByTwoPropertiesOfTheSameProxy_ThenOnlyOneParentIsSet()
    {
        // Arrange
        var context = ProxyContext
            .CreateBuilder()
            .WithParents()
            .Build();

        // Act
        var parent = new Person(context)
        {
            FirstName = "Parent"
        };

        var person = new Person(context);
        person.FirstName = "Child";
        person.Mother = parent;
        person.Father = parent;

        // Assert
        var parents = parent.GetParents();
        Assert.Equal(2, parents.Count);
    }

    [Fact]
    public void WhenReferencesAreSetToNull_ThenParentIsEmpty()
    {
        // Arrange
        var context = ProxyContext
            .CreateBuilder()
            .WithParents()
            .Build();

        // Act
        var parent = new Person(context)
        {
            FirstName = "Parent"
        };

        var person = new Person(context);
        person.FirstName = "Child";
        person.Mother = parent;
        person.Father = parent;

        person.Mother = null;
        person.Father = null;

        // Assert
        var parents = parent.GetParents();
        Assert.Empty(parents);
    }

    [Fact]
    public void WhenProxyIsReferencedByTwoOtherProxies_ThenItHasTwoParents()
    {
        // Arrange
        var context = ProxyContext
            .CreateBuilder()
            .WithParents()
            .Build();

        // Act
        var mother = new Person(context);
        mother.FirstName = "Mother";

        var child1 = new Person(context)
        {
            FirstName = "Child1",
            Mother = mother
        };

        var child2 = new Person(context)
        {
            FirstName = "Child2",
            Mother = mother
        };

        // Assert
        var parents = mother.GetParents();
        Assert.Equal(2, parents.Count);
    }
}

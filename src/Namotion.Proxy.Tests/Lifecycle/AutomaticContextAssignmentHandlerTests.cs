namespace Namotion.Proxy.Tests.Lifecycle;

public class AutomaticContextAssignmentHandlerTests
{
    [Fact]
    public void WhenPropertyIsAssigned_ThenContextIsSet()
    {
        // Arrange
        var context = ProxyContext
            .CreateBuilder()
            .WithAutomaticContextAssignment()
            .Build();

        // Act
        var person = new Person(context);
        person.Mother = new Person { FirstName = "Susi" };

        // Assert
        Assert.Equal(context, ((IProxy)person.Mother).Context);
    }

    [Fact]
    public void WhenPropertyWithDeepStructureIsAssigned_ThenChildrenAlsoHaveContext()
    {
        // Arrange
        var context = ProxyContext
            .CreateBuilder()
            .WithAutomaticContextAssignment()
            .Build();

        // Act
        var grandmother = new Person
        {
            FirstName = "Grandmother"
        };

        var mother = new Person
        {
            FirstName = "Susi",
            Mother = grandmother
        };

        var person = new Person(context)
        {
            FirstName = "Child",
            Mother = mother
        };

        // Assert
        Assert.Equal(context, ((IProxy)person).Context);
        Assert.Equal(context, ((IProxy)mother).Context);
        Assert.Equal(context, ((IProxy)grandmother).Context);
    }

    [Fact]
    public void WhenPropertyWithDeepProxiesIsRemoved_ThenAllContextsAreNull()
    {
        // Arrange
        var context = ProxyContext
            .CreateBuilder()
            .WithAutomaticContextAssignment()
            .Build();

        // Act
        var grandmother = new Person
        {
            FirstName = "Grandmother"
        };

        var mother = new Person
        {
            FirstName = "Susi",
            Mother = grandmother
        };

        var person = new Person(context)
        {
            FirstName = "Child",
            Mother = mother
        };

        person.Mother = null;

        // Assert
        Assert.Equal(context, ((IProxy)person).Context);
        Assert.Null(((IProxy)mother).Context);
        Assert.Null(((IProxy)grandmother).Context);
    }

    [Fact]
    public void WhenArrayIsAssigned_ThenAllChildrenAreAttached()
    {
        // Arrange
        var context = ProxyContext
            .CreateBuilder()
            .WithAutomaticContextAssignment()
            .Build();

        // Act
        var child1 = new Person { FirstName = "Child1" };
        var child2 = new Person { FirstName = "Child2" };

        var person = new Person(context)
        {
            FirstName = "Mother",
            Children = [
                child1,
                child2
            ]
        };

        // Assert
        Assert.Equal(context, ((IProxy)person).Context);
        Assert.Equal(context, ((IProxy)child1).Context);
        Assert.Equal(context, ((IProxy)child2).Context);
    }

    [Fact]
    public void WhenUsingCircularDependencies_ThenProxiesAreAttached()
    {
        // Arrange
        var context = ProxyContext
            .CreateBuilder()
            .WithAutomaticContextAssignment()
            .Build();

        // Act
        var child1 = new Person(context) { FirstName = "Child1" };
        var child2 = new Person { FirstName = "Child2" };
        var child3 = new Person { FirstName = "Child2" };

        child1.Mother = child2;
        child2.Mother = child3;
        child3.Mother = child1;

        // Assert
        Assert.Equal(context, ((IProxy)child1).Context);
        Assert.Equal(context, ((IProxy)child2).Context);
        Assert.Equal(context, ((IProxy)child3).Context);
    }
}

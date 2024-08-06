using Namotion.Proxy.Abstractions;

namespace Namotion.Proxy.Tests.ChangeTracking;

public class PropertyChangedHandlerTests
{
    [Fact]
    public void WhenPropertyIsChanged_ThenChangeHandlerIsTriggered()
    {
        // Arrange
        var changes = new List<ProxyPropertyChanged>();
        var context = ProxyContext
            .CreateBuilder()
            .WithPropertyChangedObservable()
            .Build();

        context
            .GetPropertyChangedObservable()
            .Subscribe(changes.Add);

        // Act
        var person = new Person(context);
        person.FirstName = "Rico";

        // Assert
        Assert.Contains(changes, c => 
            c.Property.Name == "FirstName" &&
            c.OldValue is null &&
            c.NewValue?.ToString() == "Rico");
    }
}
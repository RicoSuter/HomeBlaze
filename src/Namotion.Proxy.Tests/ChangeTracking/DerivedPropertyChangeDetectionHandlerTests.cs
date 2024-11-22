using Namotion.Proxy.Abstractions;

namespace Namotion.Proxy.Tests.ChangeTracking;

public class DerivedPropertyChangeDetectionHandlerTests
{
    [Fact]
    public void WhenChangingPropertyWhichIsUsedInDerivedProperty_ThenDerivedPropertyIsChanged()
    {
        // Arrange
        var changes = new List<ProxyPropertyChanged>();
        var context = ProxyContext
            .CreateBuilder()
            .WithDerivedPropertyChangeDetection()
            .Build();

        context
            .GetPropertyChangedObservable()
            .Subscribe(changes.Add);

        // Act
        var person = new Person(context);
        person.FirstName = "Rico";
        person.LastName = "Suter";

        // Assert
        Assert.Contains(changes, c =>
            c.Property.Name == nameof(Person.FullName) &&
            c.OldValue?.ToString() == " " &&
            c.NewValue?.ToString() == "Rico ");

        Assert.Contains(changes, c =>
            c.Property.Name == nameof(Person.FullName) &&
            c.OldValue?.ToString() == "Rico " &&
            c.NewValue?.ToString() == "Rico Suter");

        Assert.Contains(changes, c =>
            c.Property.Name == nameof(Person.FullNameWithPrefix) &&
            c.NewValue?.ToString() == "Mr. Rico Suter");
    }
}
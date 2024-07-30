using System.ComponentModel.DataAnnotations;

namespace Namotion.Proxy.Tests.Validation;

public class ProxyValidationHandlerTests
{
    [Fact]
    public void ShouldValidateProperty()
    {
        // Arrange
        var context = ProxyContext
            .CreateBuilder()
            .WithPropertyValidation()
            .WithDataAnnotationValidation()
            .Build();

        // Act
        var person = new Person(context)
        {
            FirstName = "Rico" // allowed
        };

        // Assert
        Assert.Throws<ValidationException>(() =>
        {
            person.FirstName = "Suter"; // not allowed
        });
        Assert.Equal("Rico", person.FirstName);
    }
}

using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MudBlazor;

namespace HomeBlaze.Dynamic.Tests
{
    public class DynamicThingTests
    {
        [Fact]
        public void WhenCallingDefaultInterfaceProperties_ThenTheyDoNotThrow()
        {
            // Arrange
            var typeManagerMock = new Mock<ITypeManager>();
            typeManagerMock
                .Setup(m => m.ThingInterfaces)
                .Returns(new Type[]
                {
                    typeof(IDoorSensor)
                });

            var eventManagerMock = new Mock<IEventManager>();
            eventManagerMock
                .Setup(m => m.Subscribe(It.IsAny<IObserver<IEvent>>()))
                .Returns<IDisposable>(null);

            var thing = new DynamicThing(null!, typeManagerMock.Object, eventManagerMock.Object, NullLogger<DynamicThing>.Instance)
            {
                ThingInterfaceName = typeof(IDoorSensor).FullName
            };

            // Act
            var doorSensor = thing.Thing as IDoorSensor;

            // Assert
            Assert.Equal("Default", doorSensor?.IconColor);
            Assert.NotNull(doorSensor?.IconName);
        }
    }
}
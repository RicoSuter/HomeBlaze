using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MudBlazor;
using Namotion.Devices.Abstractions.Messages;

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
                .Returns(
                [
                    typeof(IDoorSensor)
                ]);

            var eventManagerMock = new Mock<IEventManager>();
            eventManagerMock
                .Setup(m => m.Subscribe(It.IsAny<IObserver<IEvent>>()))
                .Returns<IDisposable>(null!);

            var thing = new DynamicThing(null!, typeManagerMock.Object, eventManagerMock.Object, NullLogger<DynamicThing>.Instance)
            {
                ThingInterfaceName = typeof(IDoorSensor).FullName
            };

            // Act
            var doorSensor = thing.Thing as IDoorSensor;

            // Assert
            Assert.False(string.IsNullOrEmpty(doorSensor?.IconName));
            Assert.NotNull(doorSensor?.IconName);
        }
    }
}
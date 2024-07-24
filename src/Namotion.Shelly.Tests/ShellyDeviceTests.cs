namespace Namotion.Shelly.Tests
{
    public class ShellyDeviceTests
    {
        private const string TestIpAddress = "192.168.1.125";

        [Fact]
        public async Task ShouldConnect()
        {
            var shellyDevice = ShellyDevice.Create();
            shellyDevice.IpAddress = TestIpAddress;

            await shellyDevice.StartAsync(CancellationToken.None);
            try
            {
                await WaitForAsync(() => shellyDevice.IsConnected);

                // Assert
                Assert.True(shellyDevice.IsConnected);
                Assert.NotNull(shellyDevice.Information);
                Assert.NotNull(shellyDevice.Information.Version);
            }
            finally
            {

                await shellyDevice.StopAsync(CancellationToken.None);
            }
        }

        private static async Task WaitForAsync(Func<bool> condition)
        {
            for (var i = 0; i < 100; i++)
            {
                await Task.Delay(200);

                if (condition())
                {
                    break;
                }
            }
        }
    }
}
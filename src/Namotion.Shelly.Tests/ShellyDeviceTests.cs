namespace Namotion.Shelly.Tests
{
    public class ShellyDeviceTests
    {
        private const string TestCoverIpAddress = "192.168.1.125";
        private const string TestEmIpAddress = "192.168.1.133";

        [Fact(Skip = "No CI")]
        public async Task ShouldConnectToCover()
        {
            var shellyDevice = ShellyDevice.Create();
            shellyDevice.IpAddress = TestCoverIpAddress;

            await shellyDevice.StartAsync(CancellationToken.None);
            try
            {
                await WaitForAsync(() => shellyDevice.IsConnected);

                // Assert
                Assert.True(shellyDevice.IsConnected);
                Assert.NotNull(shellyDevice.Information);
                Assert.NotNull(shellyDevice.Information.Version);

                Assert.NotNull(shellyDevice.Cover);
            }
            finally
            {

                await shellyDevice.StopAsync(CancellationToken.None);
            }
        }

        [Fact(Skip = "No CI")]
        public async Task ShouldConnectToEm()
        {
            var shellyDevice = ShellyDevice.Create();
            shellyDevice.IpAddress = TestEmIpAddress;

            await shellyDevice.StartAsync(CancellationToken.None);
            try
            {
                await WaitForAsync(() => shellyDevice.IsConnected);

                // Assert
                Assert.True(shellyDevice.IsConnected);
                Assert.NotNull(shellyDevice.Information);
                Assert.NotNull(shellyDevice.Information.Version);

                Assert.NotNull(shellyDevice.EnergyMeter);
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
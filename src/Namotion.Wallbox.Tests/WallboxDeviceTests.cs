using Namotion.Wallbox;

namespace Namotion.Shelly.Tests
{
    public class WallboxDeviceTests
    {
        [Fact(Skip = "No CI")]
        public async Task ShouldConnectToCover()
        {
            var client = new WallboxClient(new HttpClient(), "email", "password");
            var chargers = await client.GetChargersAsync(default);
            var chargerStatus = await client.GetChargerStatusAsync(chargers.First().SerialNumber!, default);
            Assert.NotNull(chargerStatus);
        }
    }
}
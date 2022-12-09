using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Gardena
{
    public class GardenaSensor : GardenaDevice, IIconProvider, IConnectedDevice, ITemperatureSensor, ISoilSensor, ILightSensor, IBatteryDevice
    {
        public override string? Id => GardenaId != null ?
            "gardena.sensor." + GardenaId :
            null;

        public string IconName => "fas fa-seedling";

        public GardenaLocation Location { get; internal set; }

        public DateTimeOffset? LastUpdated { get; internal set; }

        [State]
        public bool IsConnected => RfLinkState == "ONLINE";

        [State(Unit = StateUnit.Percent)]
        public decimal? BatteryLevel { get; internal set; }

        [State(Unit = StateUnit.DegreeCelsius)]
        public decimal? Temperature { get; internal set; }

        [State(Unit = StateUnit.Percent)]
        public decimal? SoilHumidity { get; internal set; }

        [State(Unit = StateUnit.DegreeCelsius)]
        public decimal? SoilTemperature { get; internal set; }

        [State]
        public decimal? LightLevel { get; internal set; }

        [State]
        public override string? Title { get; set; }

        [State]
        public string? BatteryState { get; private set; }

        [State]
        public decimal? RfLinkLevel { get; private set; }

        [State]
        public string? Serial { get; private set; }

        [State]
        public string? ModelType { get; private set; }

        [State]
        public string? RfLinkState { get; private set; }

        public GardenaSensor(GardenaLocation location, JObject data)
        {
            Location = location;
            Update(data);
        }

        [Operation]
        public async Task RefreshAsync(CancellationToken cancellationToken)
        {
            await Location.RefreshAsync(cancellationToken);
        }

        internal override GardenaSensor Update(JObject data)
        {
            GardenaId = data?["id"]?.Value<string>();

            SoilHumidity = data?["attributes"]?["soilHumidity"]?["value"]?.Value<decimal>() / 100m ?? SoilHumidity;
            SoilTemperature = data?["attributes"]?["soilTemperature"]?["value"]?.Value<decimal>() ?? SoilTemperature;
            Temperature = data?["attributes"]?["ambientTemperature"]?["value"]?.Value<decimal>() ?? Temperature;
            LightLevel = data?["attributes"]?["lightIntensity"]?["value"]?.Value<decimal>() ?? LightLevel;

            LastUpdated = DateTimeOffset.Now; // TODO: Use last updated for every value
            return this;
        }

        internal override GardenaSensor UpdateCommon(JObject data)
        {
            BatteryLevel = data?["attributes"]?["batteryLevel"]?["value"]?.Value<decimal>() / 100m ?? BatteryLevel;

            Title = data?["attributes"]?["name"]?["value"]?.Value<string>() ?? Title;
            BatteryState = data?["attributes"]?["batteryState"]?["value"]?.Value<string>() ?? BatteryState;
            RfLinkLevel = data?["attributes"]?["rfLinkLevel"]?["value"]?.Value<decimal>() ?? RfLinkLevel;
            Serial = data?["attributes"]?["serial"]?["value"]?.Value<string>() ?? Serial;
            ModelType = data?["attributes"]?["modelType"]?["value"]?.Value<string>() ?? ModelType;
            RfLinkState = data?["attributes"]?["rfLinkState"]?["value"]?.Value<string>() ?? RfLinkState;

            LastUpdated = DateTimeOffset.Now; // TODO: Use last updated for every value
            return this;
        }
    }
}

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Light;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HueApi.ColorConverters;
using HueApi.ColorConverters.Original.Extensions;
using HueApi.Models;
using HueApi.Models.Requests;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Philips.Hue
{
    public class HueLightbulb : HueDevice,
        IIconProvider,
        ILastUpdatedProvider,
        IConnectedThing,
        IDimmerLightbulb,
        IColorLightbulb,
        IColorTemperatureLightbulb,
        IPowerConsumptionSensor
    {
        internal Light LightResource { get; set; }

        public override string Title => $"{base.Title} " +
            $"({string.Join(", ", new string?[]
            {
                Color != null ? "Color" : null,
                ColorTemperature.HasValue ? "Temperature" : null,
                Brightness.HasValue ? "Dimmable" : null,
                IsOnOffLight ? "On/Off" : null, ModelId
            }.Where(e => e != null))})";

        public string IconName => "fas fa-lightbulb";

        public MudBlazor.Color IconColor => IsOn == true ? MudBlazor.Color.Warning : MudBlazor.Color.Default;

        public Guid ReferenceId => LightResource.Id;

        public bool IsOnOffLight => LightResource?.Type == "On/Off light";

        [State]
        public bool? IsOn => LightResource?.On.IsOn;

        [State]
        public decimal? Brightness => !IsOnOffLight ? (decimal?)LightResource?.Dimming?.Brightness / 100m : null;

        [State]
        public decimal? ColorTemperature =>
            LightResource?.ColorTemperature != null ?
            (LightResource?.ColorTemperature?.Mirek - LightResource?.ColorTemperature?.MirekSchema.MirekMinimum) /
            (decimal?)(LightResource?.ColorTemperature?.MirekSchema.MirekMaximum - LightResource?.ColorTemperature?.MirekSchema.MirekMinimum) :
            null;

        [State]
        public string? Color => LightResource.Color != null ? LightResource.ToHex(ModelId ?? "LCT001") : null;

        // From https://homeautotechs.com/philips-hue-light-models-full-list/
        [State]
        public decimal? PowerConsumption =>
            IsOn == true ? ModelId switch
            {
                "LWA001" => 9m,
                "LWA011" => 9m,
                "LWA017" => 9.5m,
                "LCA008" => 13.5m,
                "LCT015" => 9.5m,
                "LCT016" => 6.5m,
                "LCT007" => 9m,
                "LCT001" => 8.5m,
                "LCT011" => 9m,
                "LCT003" => 6.5m,
                "LCT012" => 6.5m,
                "LTW015" => 9.5m,
                "LTW010" => 9m,
                "LTG002" => 5m,
                "LTW004" => null,
                "LTW001" => null,
                "LTW011" => null,
                "LTW012" => 6m,
                "LTW013" => null,
                "LWB006" => null,
                "LWB010" => null,
                "LWB014" => null,
                "LWB004" => 9m,
                "LWA004" => 9m,
                "LWB007" => 9m,
                "LWE002" => 9m,
                "LWO001" => 9m,
                "LCL001" => 25m,
                "LCT026" => 6m,
                "LST002" => 20m,
                "LST001" => null,

                "VIYU-GU10-350-CCT-10011724" => 5m,

                _ => null,
            } :
            IsOn == false && IsConnected ? 0.5m :
            null;

        [State]
        public string? Socket =>
            ModelId switch
            {
                "LWA001" => "E26/E27",
                "LWA011" => "E26/E27",
                "LWA017" => "E26/E27",
                "LCA008" => "E26/E27",
                "LCT015" => "E26/E27",
                "LCT016" => "E26/E27",
                "LCT007" => "E26/E27",
                "LCT001" => null,
                "LCT011" => "E26/E27",
                "LCT003" => "GU10",
                "LCT012" => "E12/E14",
                "LTW015" => "E26/E27",
                "LTW010" => "E26/E27",
                "LTG002" => "GU10",
                "LTW004" => null,
                "LTW001" => null,
                "LTW011" => null,
                "LTW012" => "E12/E14",
                "LTW013" => "GU10",
                "LWB006" => "E26/E27",
                "LWB010" => null,
                "LWB014" => null,
                "LWB004" => "E26/E27",
                "LWA004" => "E26/E27",
                "LWB007" => null,
                "LWO001" => "E26/E27",
                "LWE002" => "E12/E14",
                "LCL001" => "n/a",
                "LCT026" => "n/a",
                "LST002" => "n/a",
                "LST001" => "n/a",

                "VIYU-GU10-350-CCT-10011724" => "GU10",

                _ => null,
            };

        [State]
        public decimal? Lumen =>
            IsOn == true ? ModelId switch
            {
                "LWA001" => 806m,
                "LWA011" => 806m,
                "LWA017" => 1055m,
                "LCA008" => 1600,
                "LCT015" => 85m * 9.5m,
                "LCT016" => 85m * 6.5m,
                "LCT007" => 90m * 9m,
                "LCT001" => 70m * 8.5m,
                "LCT011" => 650m,
                "LCT003" => 300m,
                "LCT012" => 450m,
                "LTW015" => 840m,
                "LTW010" => 806m,
                "LTG002" => 350m,
                "LTW004" => null,
                "LTW001" => null,
                "LTW011" => null,
                "LTW012" => 450m,
                "LTW013" => null,
                "LWB006" => null,
                "LWB010" => null,
                "LWB014" => null,
                "LWB004" => 750m,
                "LWA004" => 600m,
                "LWB007" => 750m,
                "LWO001" => 600m,
                "LWE002" => 470m,
                "LCL001" => 1600m,
                "LCT026" => 520m,
                "LST002" => 1600m,
                "LST001" => null,

                "VIYU-GU10-350-CCT-10011724" => 350m,

                _ => null,
            } :
            IsOn == false && IsConnected ? 0m :
            null;

        public HueLightbulb(Device device, ZigbeeConnectivity? zigbeeConnectivity, Light light, HueBridge bridge)
            : base(device, zigbeeConnectivity, bridge)
        {
            LightResource = light;
            Update(device, zigbeeConnectivity, light);
        }

        internal HueLightbulb Update(Device device, ZigbeeConnectivity? zigbeeConnectivity, Light light)
        {
            LightResource = light;
            Update(device, zigbeeConnectivity);
            return this;
        }

        [Operation]
        public async Task TurnOnAsync(CancellationToken cancellationToken = default)
        {
            var command = new UpdateLight()
                .TurnOn();

            var client = Bridge.CreateClient();
            var response = await client.UpdateLightAsync(LightResource.Id, command);
            if (response.Errors.Any() == false)
            {
                LightResource.On.IsOn = true;
                Bridge.ThingManager.DetectChanges(this);
            }
        }

        [Operation]
        public async Task TurnOffAsync(CancellationToken cancellationToken = default)
        {
            var command = new UpdateLight()
                .TurnOff();

            var client = Bridge.CreateClient();
            var response = await client.UpdateLightAsync(LightResource.Id, command);
            if (response.Errors.Any() == false)
            {
                LightResource.On.IsOn = false;
                Bridge.ThingManager.DetectChanges(this);
            }
        }

        public async Task DimmAsync(decimal brightness, CancellationToken cancellationToken = default)
        {
            var turnOffAfterChange = IsOn != true; // hack: needed to change brightness without turning on the lights

            var command = new UpdateLight()
                .TurnOn()
                .SetBrightness((double)(brightness * 100m));

            var client = Bridge.CreateClient();
            var response = await client.UpdateLightAsync(LightResource.Id, command);
            if (response.Errors.Any() == false && LightResource.Dimming is not null)
            {
                LightResource.Dimming.Brightness = (double)(brightness * 100m);
                Bridge.ThingManager.DetectChanges(this);
            }

            if (turnOffAfterChange)
            {
                await Task.Delay(3000);
                await TurnOffAsync(cancellationToken);
            }
        }

        public async Task ChangeColorAsync(string color, CancellationToken cancellationToken = default)
        {
            var rgbColor = new RGBColor(color);
            var command = new UpdateLight()
                .SetColor(rgbColor, ModelId ?? "LCT001");

            var client = Bridge.CreateClient();
            var response = await client.UpdateLightAsync(LightResource.Id, command);
            if (response.Errors.Any() == false)
            {
                LightResource.Color = rgbColor.ToColor();
                Bridge.ThingManager.DetectChanges(this);
            }
        }

        public async Task ChangeTemperatureAsync(decimal colorTemperature, CancellationToken cancellationToken = default)
        {
            var newColorTemperature = (int?)(
                LightResource.ColorTemperature?.MirekSchema.MirekMinimum +
                colorTemperature * (LightResource.ColorTemperature?.MirekSchema.MirekMaximum -
                                    LightResource.ColorTemperature?.MirekSchema.MirekMinimum));

            if (newColorTemperature != null && Bridge?.Bridge != null && Bridge.AppKey != null)
            {
                var command = new UpdateLight
                {
                    ColorTemperature = new ColorTemperature
                    {
                        Mirek = newColorTemperature,
                    }
                };

                var client = Bridge.CreateClient();
                var response = await client.UpdateLightAsync(LightResource.Id, command);
                if (response.Errors.Any() == false && LightResource.ColorTemperature is not null)
                {
                    LightResource.ColorTemperature.Mirek = newColorTemperature;
                    Bridge.ThingManager.DetectChanges(this);
                }
            }
        }
    }
}

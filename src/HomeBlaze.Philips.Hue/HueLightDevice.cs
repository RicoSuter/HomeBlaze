using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Devices.Light;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using MudBlazor;
using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.Original;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Philips.Hue
{
    public class HueLightDevice : IThing, IIconProvider, ILastUpdatedProvider,
        IConnectedThing, IDimmerLightbulb, IColorLightbulb, IColorTemperatureLightbulb, IPowerConsumptionSensor
    {
        private Light _light;

        public string? Id => Bridge != null ?
            "hue.light." + Bridge.BridgeId + "." + _light.UniqueId :
            null;

        public string Title => $"{_light.Name} " +
            $"({string.Join(", ", new string?[] 
            {
                Color != null ? "Color" : null, 
                ColorTemperature.HasValue ? "Temperature" : null, 
                Brightness.HasValue ? "Dimmable" : null, 
                IsOnOffLight ? "On/Off" : null, ModelId 
            }.Where(e => e != null))})";

        public string IconName => "fas fa-lightbulb";

        public Color IconColor => IsOn == true ? MudBlazor.Color.Warning : MudBlazor.Color.Default;

        public HueBridge Bridge { get; private set; }

        public string ReferenceId => _light.Id;

        public DateTimeOffset? LastUpdated { get; internal set; }

        public bool IsOnOffLight => _light?.Type == "On/Off light";

        [Abstractions.Attributes.State]
        public bool IsConnected => _light?.State.IsReachable == true;

        [Abstractions.Attributes.State]
        public bool? IsOn => _light?.State.On;

        [Abstractions.Attributes.State]
        public decimal? Brightness => !IsOnOffLight ? _light?.State.Brightness / 254m : null;

        [Abstractions.Attributes.State]
        public decimal? ColorTemperature =>
            _light?.Capabilities?.Control?.ColorTemperature != null ?
            (_light?.State?.ColorTemperature - _light?.Capabilities?.Control?.ColorTemperature.Min) /
            (decimal?)(_light?.Capabilities?.Control?.ColorTemperature.Max - _light?.Capabilities?.Control?.ColorTemperature.Min) :
            null;

        [Abstractions.Attributes.State]
        public string? Color => !string.IsNullOrEmpty(_light?.Capabilities?.Control?.ColorGamutType) ?
            _light?.ToRGBColor().ToHex() :
            null;

        [Abstractions.Attributes.State]
        public string? Type => _light?.Type;

        [Abstractions.Attributes.State]
        public string? ManufacturerName => _light?.ManufacturerName;

        [Abstractions.Attributes.State]
        public string? ModelId => _light?.ModelId;

        // From https://homeautotechs.com/philips-hue-light-models-full-list/
        [Abstractions.Attributes.State]
        public decimal? PowerConsumption =>
            IsOn == true ? (_light?.ModelId) switch
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

        [Abstractions.Attributes.State]
        public string? Socket =>
            _light?.ModelId switch
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

        [Abstractions.Attributes.State]
        public decimal? Lumen =>
            IsOn == true ? (_light?.ModelId) switch
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

        public HueLightDevice(Light light, HueBridge bridge)
        {
            Bridge = bridge;
            _light = light;
            Update(light);
        }

        internal HueLightDevice Update(Light light)
        {
            _light = light;
            LastUpdated = light != null ? DateTimeOffset.Now : null;
            return this;
        }

        public async Task TurnOnAsync(CancellationToken cancellationToken = default)
        {
            var client = CreateClient();

            var command = new LightCommand();
            command.On = true;

            var results = await client.SendCommandAsync(command, new[] { _light.Id });
            if (results.HasErrors() == false)
            {
                _light.State.On = true;
            }
        }

        public async Task TurnOffAsync(CancellationToken cancellationToken = default)
        {
            var client = CreateClient();

            var command = new LightCommand();
            command.On = false;

            var results = await client.SendCommandAsync(command, new[] { _light.Id });
            if (results.HasErrors() == false)
            {
                _light.State.On = false;
            }
        }

        public async Task DimmAsync(decimal brightness, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();

            var brightnessByte = (byte)(brightness * 254);
            var turnOffAfterChange = IsOn != true; // hack: needed to change brightness without turning on the lights

            var command = new LightCommand();
            command.On = true;
            command.Brightness = brightnessByte;

            var results = await client.SendCommandAsync(command, new[] { _light.Id });
            if (results.HasErrors() == false)
            {
                _light.State.Brightness = brightnessByte;
            }

            if (turnOffAfterChange)
            {
                await Task.Delay(3000);
                await TurnOffAsync(cancellationToken);
            }
        }

        public async Task ChangeColorAsync(string color, CancellationToken cancellationToken = default)
        {
            var client = CreateClient();

            var command = new LightCommand();
            command.SetColor(new RGBColor(color), _light.ModelId);

            await client.SendCommandAsync(command, new[] { _light.Id });
        }

        public async Task ChangeTemperatureAsync(decimal colorTemperature, CancellationToken cancellationToken = default)
        {
            var newColorTemperature = (int?)(
                _light.Capabilities?.Control?.ColorTemperature?.Min +
                colorTemperature * (_light.Capabilities?.Control?.ColorTemperature?.Max -
                                    _light.Capabilities?.Control?.ColorTemperature?.Min));

            if (newColorTemperature != null && Bridge?.Bridge != null && Bridge.AppKey != null)
            {
                var client = new LocalHueClient(Bridge.Bridge.IpAddress);
                client.Initialize(Bridge.AppKey);

                var command = new LightCommand();
                command.ColorTemperature = newColorTemperature.Value;

                var results = await client.SendCommandAsync(command, new[] { _light.Id });
                if (results.HasErrors() == false)
                {
                    _light.State.ColorTemperature = (int)(colorTemperature * 500m);
                }
            }
        }

        private LocalHueClient CreateClient()
        {
            if ((Bridge?.AppKey) == null || (Bridge?.Bridge) == null)
            {
                throw new InvalidOperationException("Bridge must not be null.");
            }

            var client = new LocalHueClient(Bridge.Bridge.IpAddress);
            client.Initialize(Bridge.AppKey);
            return client;
        }
    }
}

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Light;
using HomeBlaze.Abstractions.Presentation;
using Q42.HueApi.Models.Groups;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Philips.Hue
{
    public class HueGroupDevice : IThing, IIconProvider, ILastUpdatedProvider,
        ILightbulb, IDimmerLightbulb, IColorLightbulb, IColorTemperatureLightbulb, IVirtualThing
    {
        private Group _group;

        public string? Id => Bridge != null ?
            "hue.group." + Bridge.BridgeId + "." + _group.Id :
            null;

        public string Title => _group.Name;

        public string ReferenceId => _group.Id;

        public HueBridge Bridge { get; }

        public string IconName => "fas fa-layer-group";

        public MudBlazor.Color IconColor => IsLightOn == true ? MudBlazor.Color.Warning : MudBlazor.Color.Default;

        public DateTimeOffset? LastUpdated { get; private set; }

        [State]
        public HueLightDevice[] Lights { get; private set; } = new HueLightDevice[0];

        public bool? IsLightOn => Lights.Any(l => l.IsLightOn == true);

        public decimal? Brightness => Lights
            .Where(l => l.Brightness != null)
            .Average(l => l.Brightness);

        public string? Color =>
            Lights.GroupBy(d => d.Color).Count() == 1 ?
            Lights.FirstOrDefault()?.Color : null;

        public decimal? Lumen => Lights.Sum(d => d.Lumen);

        public decimal? ColorTemperature => Lights
            .Where(l => l.ColorTemperature != null)
            .Average(l => l.ColorTemperature);

        public HueGroupDevice(Group group, HueLightDevice[] lights, HueBridge bridge)
        {
            Bridge = bridge;
            _group = group;
            Update(group, lights);
        }

        internal HueGroupDevice Update(Group group, HueLightDevice[] lights)
        {
            _group = group;
            Lights = lights;
            LastUpdated = group != null ? DateTimeOffset.Now : null;
            return this;
        }

        public async Task TurnLightOnAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAll(Lights.Select(l => l.TurnLightOnAsync(cancellationToken)));
        }

        public async Task TurnLightOffAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAll(Lights.Select(l => l.TurnLightOffAsync(cancellationToken)));
        }

        public async Task DimmAsync(decimal brightness, CancellationToken cancellationToken)
        {
            await Task.WhenAll(Lights.Select(l => l.DimmAsync(brightness, cancellationToken)));
        }

        public async Task ChangeColorAsync(string color, CancellationToken cancellationToken)
        {
            await Task.WhenAll(Lights.Select(l => l.ChangeColorAsync(color, cancellationToken)));
        }

        public async Task ChangeTemperatureAsync(decimal colorTemperature, CancellationToken cancellationToken)
        {
            await Task.WhenAll(Lights.Select(l => l.ChangeTemperatureAsync(colorTemperature, cancellationToken)));
        }
    }
}

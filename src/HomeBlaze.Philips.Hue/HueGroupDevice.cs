using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Light;
using HomeBlaze.Abstractions.Presentation;
using HueApi.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Philips.Hue
{
    public class HueGroupDevice : IThing, IIconProvider, ILastUpdatedProvider,
        //ILightbulb, IDimmerLightbulb, IColorLightbulb, IColorTemperatureLightbulb, 
        IVirtualThing
    {
        private HueResource _group;

        public string Id => Bridge.Id + "/groups/" + _group.Id;

        public string Title => _group?.Metadata?.Name ?? "n/a";

        public Guid ReferenceId => _group.Id;

        public HueBridge Bridge { get; }

        public string IconName => "fas fa-layer-group";

        public MudBlazor.Color IconColor => IsOn == true ? MudBlazor.Color.Warning : MudBlazor.Color.Default;

        public DateTimeOffset? LastUpdated { get; private set; }

        [State]
        public HueLightDevice[] Lights { get; private set; } = new HueLightDevice[0];

        public bool? IsOn => Lights.Any(l => l.IsOn == true);

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

        public HueGroupDevice(HueResource group, HueLightDevice[] lights, HueBridge bridge)
        {
            Bridge = bridge;
            _group = group;
            Update(group, lights);
        }

        internal HueGroupDevice Update(HueResource group, HueLightDevice[] lights)
        {
            _group = group;
            Lights = lights;
            LastUpdated = group != null ? DateTimeOffset.Now : null;
            return this;
        }

        //public async Task TurnOnAsync(CancellationToken cancellationToken = default)
        //{
        //    await Task.WhenAll(Lights.Select(l => l.TurnOnAsync(cancellationToken)));
        //}

        //public async Task TurnOffAsync(CancellationToken cancellationToken = default)
        //{
        //    await Task.WhenAll(Lights.Select(l => l.TurnOffAsync(cancellationToken)));
        //}

        //public async Task DimmAsync(decimal brightness, CancellationToken cancellationToken = default)
        //{
        //    await Task.WhenAll(Lights.Select(l => l.DimmAsync(brightness, cancellationToken)));
        //}

        //public async Task ChangeColorAsync(string color, CancellationToken cancellationToken = default)
        //{
        //    await Task.WhenAll(Lights.Select(l => l.ChangeColorAsync(color, cancellationToken)));
        //}

        //public async Task ChangeTemperatureAsync(decimal colorTemperature, CancellationToken cancellationToken = default)
        //{
        //    await Task.WhenAll(Lights.Select(l => l.ChangeTemperatureAsync(colorTemperature, cancellationToken)));
        //}
    }
}

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Light;
using HomeBlaze.Abstractions.Presentation;
using HueApi.Models;
using HueApi.Models.Requests;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Philips.Hue
{
    public class HueGroupDevice : 
        IThing, 
        IIconProvider, 
        ILastUpdatedProvider,
        ILightbulb,
        IDimmerLightbulb,
        IVirtualThing
    {
        private HueResource _group;

        internal GroupedLight? GroupedLight { get; private set; }

        public string Id => Bridge.Id + "/groups/" + _group.Id;

        public string Title => _group?.Metadata?.Name ?? "n/a";

        public Guid ResourceId => _group.Id;

        public HueBridge Bridge { get; }

        public string IconName => "fas fa-layer-group";

        public MudBlazor.Color IconColor => IsOn == true ? MudBlazor.Color.Warning : MudBlazor.Color.Default;

        public DateTimeOffset? LastUpdated { get; private set; }

        [State]
        public HueLightDevice[] Lights { get; private set; } = new HueLightDevice[0];

        [State]
        public bool? IsOn => GroupedLight?.On.IsOn;

        [State]
        public decimal? Brightness => (decimal?)GroupedLight?.Dimming?.Brightness / 100m;

        [State]
        public decimal? Lumen => Lights.Sum(d => d.Lumen);

        public HueGroupDevice(HueResource group, GroupedLight? groupedLight, HueLightDevice[] lights, HueBridge bridge)
        {
            Bridge = bridge;

            _group = group;
            GroupedLight = groupedLight;

            Update(group, groupedLight, lights);
        }

        internal HueGroupDevice Update(HueResource group, GroupedLight? groupedLight, HueLightDevice[] lights)
        {
            _group = group; 
            GroupedLight = groupedLight;

            Lights = lights;
            LastUpdated = group != null ? DateTimeOffset.Now : null;

            return this;
        }

        [Operation]
        public async Task TurnOnAsync(CancellationToken cancellationToken = default)
        {
            if (GroupedLight is not null)
            {
                var command = new UpdateGroupedLight()
                    .TurnOn();

                var client = Bridge.CreateClient();
                await client.UpdateGroupedLightAsync(GroupedLight.Id, command);
            }
        }

        [Operation]
        public async Task TurnOffAsync(CancellationToken cancellationToken = default)
        {
            if (GroupedLight is not null)
            {
                var command = new UpdateGroupedLight()
                    .TurnOff();

                var client = Bridge.CreateClient();
                await client.UpdateGroupedLightAsync(GroupedLight.Id, command);
            }
        }

        public async Task DimmAsync(decimal brightness, CancellationToken cancellationToken = default)
        {
            if (GroupedLight is not null)
            {
                var turnOffAfterChange = IsOn != true; // hack: needed to change brightness without turning on the lights

                var command = new UpdateGroupedLight()
                    .TurnOn()
                    .SetBrightness((double)(brightness * 100m));

                var client = Bridge.CreateClient();
                await client.UpdateGroupedLightAsync(GroupedLight.Id, command);

                if (turnOffAfterChange)
                {
                    await Task.Delay(3000);
                    await TurnOffAsync(cancellationToken);
                }
            }
        }
    }
}

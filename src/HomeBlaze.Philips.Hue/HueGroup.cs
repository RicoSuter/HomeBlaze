﻿using HomeBlaze.Abstractions;
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
    public class HueGroup :
        IThing,
        IIconProvider,
        ILastUpdatedProvider,
        ILightbulb,
        IColorLightbulb,
        IColorTemperatureLightbulb,
        IDimmerLightbulb,
        IVirtualThing
    {
        internal HueResource Group { get; set; }

        internal GroupedLight? GroupedLight { get; set; }

        public string Id => Bridge.Id + "/groups/" + ResourceId;

        public string Title => Group?.Metadata?.Name ?? "n/a";

        public Guid ResourceId => Group.Id;

        public HueBridge Bridge { get; }

        public string IconName => "fas fa-layer-group";

        public string IconColor => IsOn == true ? "Warning" : "Default";

        public DateTimeOffset? LastUpdated { get; internal set; }

        [State]
        public HueLightbulb[] Lights { get; private set; } = [];

        [State]
        public bool? IsOn => GroupedLight?.On.IsOn;

        [State]
        public decimal? Lumen => Lights.Sum(d => d.Lumen);
        
        [State]
        public string? Color => 
            Lights
                .GroupBy(l => l.Color)
                .OrderBy(g => g.Count())
                .FirstOrDefault(g => g.Count() == Lights.Count())?
                .FirstOrDefault()?
                .Color;
        
        [State]
        public decimal? ColorTemperature => 
            Lights
                .GroupBy(l => l.Color)
                .OrderBy(g => g.Count())
                .FirstOrDefault(g => g.Count() == Lights.Count())?
                .FirstOrDefault()?
                .ColorTemperature;

        [State]
        public decimal? Brightness => 
            (decimal?)GroupedLight?.Dimming?.Brightness / 100m ??
            Lights
                .GroupBy(l => l.Color)
                .OrderBy(g => g.Count())
                .FirstOrDefault(g => g.Count() == Lights.Count())?
                .FirstOrDefault()?
                .Brightness;
        
        public HueGroup(HueResource group, GroupedLight? groupedLight, HueLightbulb[] lights, HueBridge bridge)
        {
            Bridge = bridge;

            Group = group;
            GroupedLight = groupedLight;

            Update(group, groupedLight, lights);
        }

        internal HueGroup Update(HueResource group, GroupedLight? groupedLight, HueLightbulb[] lights)
        {
            Group = group;
            GroupedLight = groupedLight;

            Lights = lights;
            LastUpdated = DateTimeOffset.Now;

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
                var response = await client.UpdateGroupedLightAsync(GroupedLight.Id, command);
                if (response.Errors.Any() == false)
                {
                    GroupedLight.On.IsOn = true;
                    Bridge.DetectChanges(this);
                }
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
                var response = await client.UpdateGroupedLightAsync(GroupedLight.Id, command);
                if (response.Errors.Any() == false)
                {
                    GroupedLight.On.IsOn = false;
                    Bridge.DetectChanges(this);
                }
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
                var response = await client.UpdateGroupedLightAsync(GroupedLight.Id, command);
                if (response.Errors.Any() == false && GroupedLight.Dimming is not null)
                {
                    GroupedLight.Dimming.Brightness = (double)(brightness * 100m);
                    Bridge.DetectChanges(this);
                }

                if (turnOffAfterChange)
                {
                    await Task.Delay(3000);
                    await TurnOffAsync(cancellationToken);
                }
            }
        }
        
        public async Task ChangeColorAsync(string color, CancellationToken cancellationToken = default)
        {
            foreach (var light in Lights)
            {
                await light.ChangeColorAsync(color, cancellationToken);
            }
        }
        
        public async Task ChangeTemperatureAsync(decimal colorTemperature, CancellationToken cancellationToken = default)
        {
            foreach (var light in Lights)
            {
                await light.ChangeTemperatureAsync(colorTemperature, cancellationToken);
            }
        }
    }
}

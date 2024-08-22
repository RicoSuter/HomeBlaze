using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Components.Abstractions;
using HomeBlaze.Philips.Hue;
using HomeBlaze.Services.Abstractions;
using Namotion.Lights;
using System;
using System.Linq;

namespace Namotion
{
    public class HueBridgeExtension :
        ExtensionThing<HueBridge>,
        ILastUpdatedProvider,
        IPageProvider
    {
        public DateTimeOffset? LastUpdated => ExtendedThing?.LastUpdated;

        public string? PageTitle => "Lights";

        public Type? PageComponentType => typeof(LightsPage);

        [State]
        public long? NumberOfLightbulbs => ExtendedThing?
            .Lights
            .Count();

        [Operation]
        public void BackupLightState()
        {
        }

        [Operation]
        public void RestoreLightState()
        {
        }

        public HueBridgeExtension(IThingManager thingManager, IEventManager eventManager)
            : base(thingManager, eventManager)
        {
            Title = "Namotion Philips Hue Extensions";
        }
    }
}

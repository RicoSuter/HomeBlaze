using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Philips.Hue;
using HomeBlaze.Services.Things;
using System;
using System.Linq;

namespace Namotion
{
    public class HueBridgeExtension : ExtensionThing<HueBridge>, ILastUpdatedProvider
    {
        public DateTimeOffset? LastUpdated => ExtendedThing?.LastUpdated;

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

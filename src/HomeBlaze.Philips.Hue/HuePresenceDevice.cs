using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HueApi.Models;
using System;

namespace HomeBlaze.Philips.Hue
{
    public class HuePresenceDevice : IThing, 
        IIconProvider, 
        //ILastUpdatedProvider,
        IPresenceSensor, 
        IBatteryDevice
    {
        private Device _sensor;

        public string Id => Bridge.Id + "/sensors/" + _sensor.Id;

        public string Title => _sensor?.Metadata?.Name + " (Motion Sensor)";

        public string IconName => "fas fa-running";

        public HueBridge Bridge { get; private set; }

        public Guid ReferenceId => _sensor.Id;

        //public DateTimeOffset? LastUpdated => _sensor?.State.Lastupdated;

        // TODO: Add last updated property

        [State]
        public bool? IsPresent => null; // _sensor?.State.Presence;

        [State]
        public decimal? BatteryLevel => null; // _sensor?.Config.Battery / 100m;

        [State]
        public string? Type => _sensor?.Type;

        [State]
        public string? ManufacturerName => _sensor?.ProductData?.ManufacturerName;

        [State]
        public string? ModelId => _sensor?.ProductData?.ModelId;

        public HuePresenceDevice(Device sensor, HueBridge bridge)
        {
            Bridge = bridge;
            _sensor = sensor;
            Update(sensor);
        }

        internal HuePresenceDevice Update(Device sensor)
        {
            _sensor = sensor;
            return this;
        }
    }
}

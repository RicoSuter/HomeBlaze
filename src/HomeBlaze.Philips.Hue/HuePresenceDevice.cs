using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using Q42.HueApi.Models;
using System;

namespace HomeBlaze.Philips.Hue
{
    public class HuePresenceDevice : IThing, IIconProvider, ILastUpdatedProvider,
        IPresenceSensor, IBatteryDevice
    {
        private Sensor _sensor;

        public string? Id => Bridge != null ?
            "hue.presence." + Bridge.BridgeId + "." + _sensor.UniqueId :
            null;

        public string Title => _sensor.Name + " (Motion Sensor)";

        public string IconName => "fas fa-running";

        public HueBridge Bridge { get; private set; }

        public string ReferenceId => _sensor.Id;

        public DateTimeOffset? LastUpdated => _sensor?.State.Lastupdated;

        // TODO: Add last updated property

        [State]
        public bool? IsPresent => _sensor?.State.Presence;

        [State]
        public decimal? BatteryLevel => _sensor?.Config.Battery / 100m;

        [State]
        public string? Type => _sensor?.Type;

        [State]
        public string? ManufacturerName => _sensor?.ManufacturerName;

        [State]
        public string? ModelId => _sensor?.ModelId;

        public HuePresenceDevice(Sensor sensor, HueBridge bridge)
        {
            Bridge = bridge;
            _sensor = sensor;
            Update(sensor);
        }

        internal HuePresenceDevice Update(Sensor sensor)
        {
            _sensor = sensor;
            return this;
        }
    }
}

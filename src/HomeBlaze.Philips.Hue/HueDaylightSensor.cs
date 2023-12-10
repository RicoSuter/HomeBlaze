using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Presentation;
using HueApi.Models;
using System;

namespace HomeBlaze.Philips.Hue
{
    public class HueDaylightSensor :
        IThing,
        IIconProvider
        //ILastUpdatedProvider,
        //IDaylightSensor
    {
        private Device _sensor;

        public string Id => Bridge.Id + "/sensors/" + _sensor.Id;

        public string Title => _sensor?.Metadata?.Name ?? "n/a";

        public string IconName => "fas fa-sun";

        public HueBridge Bridge { get; private set; }

        public Guid ReferenceId => _sensor.Id;

        //[State]
        //public bool? IsDaylight => _sensor?.State.Daylight;

        //[State]
        //public DateTimeOffset? LastUpdated => _sensor?.State.Lastupdated;

        public HueDaylightSensor(Device sensor, HueBridge bridge)
        {
            Bridge = bridge;
            _sensor = sensor;
            Update(sensor);
        }

        internal HueDaylightSensor Update(Device sensor)
        {
            _sensor = sensor;
            return this;
        }
    }
}

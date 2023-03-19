using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using Q42.HueApi.Models;
using System;

namespace HomeBlaze.Philips.Hue
{
    public class HueDaylightSensor : IThing, IIconProvider, IDaylightSensor
    {
        private Sensor _sensor;

        public string? Id => Bridge != null ?
            "hue.daylight." + Bridge.BridgeId + "." + _sensor.Id :
            null;

        public string Title => _sensor.Name;

        public string IconName => "fas fa-sun";

        public HueBridge Bridge { get; private set; }

        public string ReferenceId => _sensor.Id;

        [State]
        public bool? IsDaylight => _sensor?.State.Daylight;

        [State]
        public DateTimeOffset? LastUpdated => _sensor?.State.Lastupdated;

        public HueDaylightSensor(Sensor sensor, HueBridge bridge)
        {
            Bridge = bridge;
            _sensor = sensor;
            Update(sensor);
        }

        internal HueDaylightSensor Update(Sensor sensor)
        {
            _sensor = sensor;
            return this;
        }
    }
}

using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using Q42.HueApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeBlaze.Philips.Hue
{
    public class HueLightSensor : IThing, IIconProvider, ILastUpdatedProvider,
        ILightSensor, IDaylightSensor
    {
        private Sensor _sensor;
        private Sensor? _parentSensor;

        public string Id => Bridge.Id + "/sensors/" + (_sensor.UniqueId ?? _sensor.Id);

        public string Title => (_parentSensor?.Name ?? _sensor.Name) + " (Ambient Light Sensor)";

        public string IconName => "fas fa-sun";

        public HueBridge Bridge { get; private set; }

        public string ReferenceId => _sensor.Id;

        public DateTimeOffset? LastUpdated => _sensor.State.Lastupdated;

        [State]
        public decimal? LightLevel => _sensor.State.LightLevel != null ?
            (decimal)Math.Pow(10, (_sensor.State.LightLevel.Value - 1.0) / 10000.0) : null;

        [State]
        public bool? IsDaylight => _sensor.State.Daylight;

        public HueLightSensor(Sensor sensor, IEnumerable<Sensor> allSensors, HueBridge bridge)
        {
            Bridge = bridge;
            _sensor = sensor;
            Update(sensor, allSensors);
        }

        internal HueLightSensor Update(Sensor sensor, IEnumerable<Sensor> allSensors)
        {
            _sensor = sensor;
            _parentSensor = allSensors
                .Where(s => s.Type == "ZLLPresence" && s.UniqueId.Split('-').First() == sensor.UniqueId.Split('-').First())
                .Single();

            return this;
        }
    }
}

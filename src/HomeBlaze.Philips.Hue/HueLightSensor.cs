using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HueApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeBlaze.Philips.Hue
{
    public class HueLightSensor : IThing, 
        IIconProvider 
        //ILastUpdatedProvider,
        //ILightSensor, 
        //IDaylightSensor
    {
        private Device _sensor;
        private Device? _parentSensor;

        public string Id => Bridge.Id + "/sensors/" + _sensor.Id;

        public string Title => (_parentSensor?.Metadata?.Name ?? _sensor?.Metadata?.Name) + " (Ambient Light Sensor)";

        public string IconName => "fas fa-sun";

        public HueBridge Bridge { get; private set; }

        public Guid ReferenceId => _sensor.Id;

        //public DateTimeOffset? LastUpdated => _sensor.State.Lastupdated;

        //[State]
        //public decimal? LightLevel => _sensor.State.LightLevel != null ?
        //    (decimal)Math.Pow(10, (_sensor.State.LightLevel.Value - 1.0) / 10000.0) : null;

        //[State]
        //public bool? IsDaylight => _sensor.State.Daylight;

        public HueLightSensor(Device sensor, IEnumerable<Device> allSensors, HueBridge bridge)
        {
            Bridge = bridge;
            _sensor = sensor;
            Update(sensor, allSensors);
        }

        internal HueLightSensor Update(Device sensor, IEnumerable<Device> allSensors)
        {
            _sensor = sensor;
            _parentSensor = allSensors
                .Where(s => s.Type == "ZLLPresence" && s.IdV1?.Split('-').First() == sensor.IdV1?.Split('-').First())
                .Single();

            return this;
        }
    }
}

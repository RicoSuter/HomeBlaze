using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using Q42.HueApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Philips.Hue
{
    public class HueTemperatureDevice :
        IIconProvider, ILastUpdatedProvider,
        ITemperatureSensor, IBatteryDevice
    {
        private Sensor _sensor;
        private Sensor? _parentSensor;

        public string Id => Bridge.Id + "/sensors/" + (_sensor.UniqueId ?? _sensor.Id);

        public string Title => (_parentSensor?.Name ?? _sensor.Name) + " (Temperature Sensor)";

        public string IconName => "fas fa-cloud";

        public HueBridge Bridge { get; private set; }

        public string ReferenceId => _sensor.Id;

        public DateTimeOffset? LastUpdated => _sensor.State.Lastupdated;

        [State]
        public decimal? Temperature => _sensor.State.Temperature / 100m;

        [State]
        public decimal? BatteryLevel => _sensor.Config.Battery / 100m;

        [State]
        public string? Type => _sensor.Type;

        [State]
        public string? ManufacturerName => _sensor.ManufacturerName;

        [State]
        public string? ModelId => _sensor.ModelId;

        public HueTemperatureDevice(Sensor sensor, IEnumerable<Sensor> allSensors, HueBridge bridge)
        {
            Bridge = bridge;
            _sensor = sensor;

            Update(sensor, allSensors);
        }

        internal HueTemperatureDevice Update(Sensor sensor, IEnumerable<Sensor> allSensors)
        {
            _sensor = sensor;
            _parentSensor = allSensors
                .Where(s => s.Type == "ZLLPresence" && s.UniqueId.Split('-').First() == sensor.UniqueId.Split('-').First())
                .Single();
         
            return this;
        }

        public Task TurnOnAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task TurnOffAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

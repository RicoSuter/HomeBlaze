using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Presentation;
using HueApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Philips.Hue
{
    public class HueTemperatureDevice :
        IIconProvider,
        //ILastUpdatedProvider,
        //ITemperatureSensor, 
        IBatteryDevice
    {
        private Device _sensor;
        private Device? _parentSensor;

        public string Id => Bridge.Id + "/sensors/" + _sensor.Id;

        public string Title => (_parentSensor?.Metadata?.Name ?? _sensor.Metadata?.Name) + " (Temperature Sensor)";

        public string IconName => "fas fa-cloud";

        public HueBridge Bridge { get; private set; }

        public Guid ReferenceId => _sensor.Id;

        //public DateTimeOffset? LastUpdated => _sensor.Metadata.State.Lastupdated;

        //[State]
        //public decimal? Temperature => _sensor.State.Temperature / 100m;

        [State]
        public decimal? BatteryLevel => null; // _sensor.Config.Battery / 100m;

        [State]
        public string? Type => _sensor.Type;

        [State]
        public string? ManufacturerName => _sensor.ProductData.ManufacturerName;

        [State]
        public string? ModelId => _sensor.ProductData.ModelId;

        public HueTemperatureDevice(Device sensor, IEnumerable<Device> allSensors, HueBridge bridge)
        {
            Bridge = bridge;
            _sensor = sensor;

            Update(sensor, allSensors);
        }

        internal HueTemperatureDevice Update(Device sensor, IEnumerable<Device> allSensors)
        {
            _sensor = sensor;
            _parentSensor = allSensors
                .Where(s => s.Type == "ZLLPresence" && s.IdV1?.Split('-').First() == sensor.IdV1?.Split('-').First())
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

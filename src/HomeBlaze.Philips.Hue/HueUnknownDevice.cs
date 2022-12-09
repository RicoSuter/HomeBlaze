using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using Newtonsoft.Json;
using Q42.HueApi.Models;
using System;

namespace HomeBlaze.Philips.Hue
{
    public class HueUnknownDevice : IThing, ILastUpdatedProvider,
        IUnknownDevice
    {
        private Sensor _sensor;

        public string? Id => Bridge != null ?
            "hue.unknown." + Bridge.BridgeId + "." + (_sensor.UniqueId ?? _sensor.Id) :
            null;

        public string Title => _sensor.Name;

        public DateTimeOffset? LastUpdated { get; internal set; }

        public HueBridge Bridge { get; }

        public string ReferenceId => _sensor.Id;


        [State]
        public string? ManufacturerName => _sensor?.ManufacturerName;

        [State]
        public string? ModelId => _sensor.ModelId;

        [State]
        public string? State => JsonConvert.SerializeObject(_sensor.State);

        // TODO: Move away from unknown device!
        [State]
        public decimal? Illuminance => _sensor?.State?.LightLevel != null ?
            (decimal)Math.Pow(10, (_sensor.State.LightLevel.Value - 1.0) / 10000.0) : null;

        public HueUnknownDevice(Sensor sensor, HueBridge bridge)
        {
            Bridge = bridge;
            _sensor = sensor;

            Update(sensor);
        }

        internal HueUnknownDevice Update(Sensor sensor)
        {
            _sensor = sensor;
            LastUpdated = sensor != null ? DateTimeOffset.Now : null;
            return this;
        }
    }
}

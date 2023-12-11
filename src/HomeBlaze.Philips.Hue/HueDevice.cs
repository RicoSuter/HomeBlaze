using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Networking;
using HueApi;
using HueApi.Models;
using System;

namespace HomeBlaze.Philips.Hue
{
    public class HueDevice :
        IThing,
        ILastUpdatedProvider,
        IConnectedThing,
        IUnknownDevice
    {
        private Device _device;
        private ZigbeeConnectivity? _zigbeeConnectivity;

        public string Id => Bridge.Id + "/resources/" + _device.Id;

        public virtual string Title => _device?.Metadata?.Name ?? "n/a";

        public DateTimeOffset? LastUpdated { get; internal set; }

        public HueBridge Bridge { get; }

        public Guid DeviceId => _device.Id;

        [State]
        public bool? IsCertified => _device.ProductData.Certified;

        [State]
        public string? ProductName => _device.ProductData.ProductName;

        [State]
        public string? HardwarePlatformType => _device.ProductData.HardwarePlatformType;

        [State]
        public string? SoftwareVersion => _device.ProductData.SoftwareVersion;

        [State]
        public string? ManufacturerName => _device.ProductData.ManufacturerName;

        [State]
        public string? ModelId => _device.ProductData.ModelId;

        public bool IsConnected => _zigbeeConnectivity == null || _zigbeeConnectivity.Status == ConnectivityStatus.connected;

        public HueDevice(Device device, ZigbeeConnectivity? zigbeeConnectivity, HueBridge bridge)
        {
            Bridge = bridge;

            _device = device;
            _zigbeeConnectivity = zigbeeConnectivity;

            Update(device, zigbeeConnectivity);
        }

        internal HueDevice Update(Device device, ZigbeeConnectivity? zigbeeConnectivity)
        {
            _device = device;
            _zigbeeConnectivity = zigbeeConnectivity;
          
            LastUpdated = device != null ? DateTimeOffset.Now : null;
         
            return this;
        }
    }
}

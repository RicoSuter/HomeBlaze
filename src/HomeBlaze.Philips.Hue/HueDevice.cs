using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HueApi.Models;
using System;

namespace HomeBlaze.Philips.Hue
{
    public class HueDevice :
        IThing,
        ILastUpdatedProvider,
        IConnectedThing,
        IUnknownDevice,
        IIconProvider
    {
        internal Device Device { get; private set; }

        internal ZigbeeConnectivity? ZigbeeConnectivity { get; private set; }

        public string Id => Bridge.Id + "/devices/" + ResourceId;

        public virtual string Title => Device?.Metadata?.Name ?? "n/a";

        public DateTimeOffset? LastUpdated { get; internal set; }

        public HueBridge Bridge { get; }

        public Guid ResourceId => Device.Id;

        public virtual string IconName => "fas fa-question-circle";

        public virtual MudBlazor.Color IconColor => IsConnected ? MudBlazor.Color.Default : MudBlazor.Color.Error;

        [State]
        public bool? IsCertified => Device.ProductData.Certified;

        [State]
        public string? ProductName => Device.ProductData.ProductName;

        [State]
        public string? HardwarePlatformType => Device.ProductData.HardwarePlatformType;

        [State]
        public string? SoftwareVersion => Device.ProductData.SoftwareVersion;

        [State]
        public string? ManufacturerName => Device.ProductData.ManufacturerName;

        [State]
        public string? ModelId => Device.ProductData.ModelId;

        public bool IsConnected => ZigbeeConnectivity == null || ZigbeeConnectivity.Status == ConnectivityStatus.connected;

        public HueDevice(Device device, ZigbeeConnectivity? zigbeeConnectivity, HueBridge bridge)
        {
            Bridge = bridge;

            Device = device;
            ZigbeeConnectivity = zigbeeConnectivity;

            Update(device, zigbeeConnectivity);
        }

        internal HueDevice Update(Device device, ZigbeeConnectivity? zigbeeConnectivity)
        {
            Device = device;
            ZigbeeConnectivity = zigbeeConnectivity;

            LastUpdated = DateTimeOffset.Now;
            return this;
        }
    }
}

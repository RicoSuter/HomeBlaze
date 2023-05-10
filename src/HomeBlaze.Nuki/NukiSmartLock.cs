using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Nuki.Model;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Nuki
{
    public class NukiSmartLock : IThing, IIconProvider, ILastUpdatedProvider,
        IConnectedThing, IDoorLock, IDoorSensor, IBatteryDevice
    {
        private NukiDevice _device;

        [Configuration(IsIdentifier = true)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string? Title => _device?.Name ?? "n/a";

        public IThing Parent => Bridge;

        public string IconName => "fas fa-lock";

        [JsonIgnore]
        public NukiBridge Bridge { get; private set; }

        public DateTimeOffset? LastUpdated { get; private set; }

        // State

        [State]
        public bool IsConnected => _device.LastKnownState != null;

        [State]
        public int NukiId => _device.NukiId;

        [State]
        public decimal? BatteryLevel => _device.LastKnownState?.BatteryChargeState / 100m;

        [State]
        public DoorState? DoorState =>
            _device.LastKnownState?.DoorsensorState == 3 ? Abstractions.Sensors.DoorState.Open :
            _device.LastKnownState?.DoorsensorState == 2 ? Abstractions.Sensors.DoorState.Close :
            null;

        [State]
        public DoorLockState? DoorLockState =>
            _device.LastKnownState?.State == 1 ? Abstractions.Devices.DoorLockState.Locked :
            _device.LastKnownState?.State == 2 ? Abstractions.Devices.DoorLockState.Unlocking :
            _device.LastKnownState?.State == 3 ? Abstractions.Devices.DoorLockState.Unlocked :
            _device.LastKnownState?.State == 4 ? Abstractions.Devices.DoorLockState.Locking :
            _device.LastKnownState?.State == 5 ? Abstractions.Devices.DoorLockState.Unlatched :
            _device.LastKnownState?.State == 6 ? Abstractions.Devices.DoorLockState.Unlocked :
            _device.LastKnownState?.State == 7 ? Abstractions.Devices.DoorLockState.Unlatching :
            null;

        public NukiSmartLock(NukiDevice device, NukiBridge bridge)
        {
            Bridge = bridge;
            Update(device);
        }

        [MemberNotNull(nameof(_device))]
        public void Update(NukiDevice device)
        {
            _device = device;
            LastUpdated = device != null ? DateTimeOffset.Now : null;
        }

        public async Task LockDoorAsync(CancellationToken cancellationToken)
        {
            if (DoorLockState == Abstractions.Devices.DoorLockState.Unlocked)
            {
                if (_device.LastKnownState != null)
                {
                    _device.LastKnownState.State = 4;
                    Bridge.ThingManager.DetectChanges(this);
                }

                using var httpClient = Bridge._httpClientFactory.CreateClient();
                await httpClient.GetAsync($"http://{Bridge.Host}/lock?nukiId={NukiId}&token=" + Bridge.AuthToken, cancellationToken);

                Bridge.ThingManager.DetectChanges(this);
            }
        }

        public async Task UnlockDoorAsync(CancellationToken cancellationToken)
        {
            if (DoorLockState == Abstractions.Devices.DoorLockState.Locked)
            {
                if (_device.LastKnownState != null)
                {
                    _device.LastKnownState.State = 2;
                    Bridge.ThingManager.DetectChanges(this);
                }

                using var httpClient = Bridge._httpClientFactory.CreateClient();
                await httpClient.GetAsync($"http://{Bridge.Host}/unlock?nukiId={NukiId}&token=" + Bridge.AuthToken, cancellationToken);

                Bridge.ThingManager.DetectChanges(this);
            }
        }
    }
}

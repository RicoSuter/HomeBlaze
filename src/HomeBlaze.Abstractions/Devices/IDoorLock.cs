using HomeBlaze.Abstractions.Attributes;

namespace HomeBlaze.Abstractions.Devices
{
    public interface IDoorLock : IThing
    {
        [State]
        bool? IsDoorLocked => DoorLockState.HasValue ? DoorLockState == Devices.DoorLockState.Locked : null;

        [State]
        DoorLockState? DoorLockState { get; }

        [Operation]
        Task LockDoorAsync(CancellationToken cancellationToken = default);

        [Operation]
        Task UnlockDoorAsync(CancellationToken cancellationToken = default);

        [Operation]
        async Task ToggleDoorLockAsync(CancellationToken cancellationToken = default)
        {
            if (DoorLockState == Devices.DoorLockState.Locked)
            {
                await UnlockDoorAsync(cancellationToken);
            }
            else if (DoorLockState == Devices.DoorLockState.Unlocked)
            {
                await LockDoorAsync(cancellationToken);
            }
        }
    }

    public enum DoorLockState
    {
        Unlocking,
        Unlocked,
        Locking,
        Locked,
        Unlatching,
        Unlatched
    }
}
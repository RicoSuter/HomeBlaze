namespace Namotion.Wallbox.Responses.GetChargerStatus
{
    public enum ChargerStatus
    {
        Unknown = 0,
        Disconnected,
        Error,
        Ready,
        Waiting,
        Locked,
        Updating,
        Scheduled,
        Paused,
        WaitingForCarDemand,
        WaitingInQueueByPowerSharing,
        WaitingInQueueByPowerBoost,
        WaitingMidFailed,
        WaitingMidSafetyMarginExceeded,
        WaitingInQueueByEcoSmart,
        Charging,
        Discharging,
        LockedCarConnected
    }
}

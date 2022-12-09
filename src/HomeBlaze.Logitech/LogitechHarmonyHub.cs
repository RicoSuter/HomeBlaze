using Baflux.Logitech.Harmony;
using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices;
using HomeBlaze.Abstractions.Inputs;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Things;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Logitech
{
    [DisplayName("Logitech Harmony Hub")]
    [ThingSetup(typeof(LogitechHarmonyHubSetup))]
    public class LogitechHarmonyHub : PollingThing, IAsyncDisposable, IConnectedDevice, IHubDevice, IActivityDevice, IActionDevice, IPowerConsumptionSensor
    {
        private DateTimeOffset _lastCheck = DateTimeOffset.MinValue;
        private readonly IThingManager _thingManager;
        private readonly ILogger<LogitechHarmonyHub> _logger;

        internal Harmony.Hub? Hub { get; private set; }

        public override string Id => "logitech.harmony.hub." + RemoteId;

        public override string Title => "Logitech Harmony Hub (" + RemoteId + ")";

        public string IconName => "fab fa-hubspot";

        [State]
        public IEnumerable<IThing> Devices { get; private set; } = Array.Empty<IThing>();

        [State]
        public IActivityDevice.DeviceActivity Activity { get; private set; }

        [State]
        public IActivityDevice.DeviceActivity[] Activities { get; private set; } = Array.Empty<IActivityDevice.DeviceActivity>();

        [State]
        public IActionDevice.DeviceAction[] Actions { get; private set; } = Array.Empty<IActionDevice.DeviceAction>();

        [State]
        public bool IsConnected { get; private set; }

        [State]
        public decimal? PowerConsumption => IsConnected ? 1.5m : 0;

        [Configuration]
        public string? IpAddress { get; set; }

        [Configuration]
        public string? RemoteId { get; set; }

        protected override TimeSpan PollingInterval => TimeSpan.FromMinutes(15);

        protected override TimeSpan FailureInterval => TimeSpan.FromSeconds(5);

        public LogitechHarmonyHub(IThingManager thingManager, ILogger<LogitechHarmonyHub> logger) 
            : base(thingManager, logger)
        {
            _thingManager = thingManager;
            _logger = logger;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (Hub == null)
                {
                    Hub = new Harmony.Hub(IpAddress!, RemoteId!);
                    Hub.OnStateDigestReceived += OnActivityProgress!;

                    await Hub.ConnectAsync(Harmony.DeviceID.GetDeviceDefault());
                }

                await RefreshAsync(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to connect to Logitech Harmony hub.");

                if (Hub != null)
                {
                    try
                    {
                        await Hub.Disconnect();
                        Hub.Dispose();
                    }
                    catch
                    {
                    }

                    Hub = null;
                }

                IsConnected = false;
                throw;
            }
        }

        public async Task ChangeActivityAsync(string activityId, CancellationToken cancellationToken)
        {
            var activity = Hub?.Activities
                .SingleOrDefault(a => a.Id == activityId);

            if (activity != null && Hub != null)
            {
                await Hub.StartActivity(activity, cancellationToken);
            }
        }

        public async Task ExecuteActionAsync(string actionId, CancellationToken cancellationToken)
        {
            var activity = Hub?.Activities
                .SingleOrDefault(a => a.Id == Activity.Id);

            if (activity != null && Hub != null)
            {
                var function = activity.Functions.SingleOrDefault(f => f.Name == actionId);
                if (function != null)
                {
                    await Hub.PressButtonAsync(function);
                }
            }
        }

        private async Task RefreshAsync(CancellationToken cancellationToken)
        {
            if (Hub == null)
            {
                return;
            }

            var syncConfiguration = DateTimeOffset.Now - _lastCheck > TimeSpan.FromMinutes(15);
            if (syncConfiguration)
            {
                await Hub.SyncConfigurationAsync(cancellationToken);
            }

            await Hub.UpdateStateAsync(cancellationToken);

            if (syncConfiguration)
            {
                Activities = Hub.Activities
                    .Select(a => new IActivityDevice.DeviceActivity
                    {
                        Id = a.Id,
                        Title = a.Label
                    })
                    .ToArray();

                Devices = Hub.Devices
                    .Select(s => Devices
                        .OfType<LogitechHarmonyDevice>()
                        .SingleOrDefault(d => d.ReferenceId == s.UUID)?.Update(s)
                        ?? new LogitechHarmonyDevice(s, this))
                    .OfType<IThing>()
                    .ToArray();
            }

            IsConnected = true;
            _lastCheck = DateTimeOffset.Now;
        }

        private void OnActivityProgress(object sender, Harmony.Events.HarmonyEventArgs<Harmony.StateDigest> e)
        {
            var runningActivity = Hub?.GetRunningActivity();
            var activity = Hub?.Activities
                .SingleOrDefault(a => a.Id == runningActivity?.Id);

            if (activity != null && runningActivity != null && runningActivity.Id != Activity.Id)
            {
                var oldActivity = Activity;

                Activity = new IActivityDevice.DeviceActivity
                {
                    Id = activity.Id,
                    Title = activity.Label
                };

                Actions = activity.Functions
                    .Select(a => new IActionDevice.DeviceAction
                    {
                        Id = a.Name,
                        Title = a.Label
                    })
                    .ToArray();

                _thingManager.DetectChanges(this);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Hub != null)
            {
                await Hub.Disconnect();
            }
        }
    }
}
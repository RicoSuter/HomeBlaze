using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Inputs;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.Logging;
using SmartGlass;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XboxWebApi.Services;

namespace HomeBlaze.Xbox
{
    [DisplayName("Microsoft Xbox")]
    public class XboxDevice : PollingThing, IIconProvider,
        IConnectedThing, IActivityDevice, INetworkAdapter
    {
        private readonly ILogger _logger;
        private readonly IThingManager _thingManager;

        private XboxApp[] _installedApps = Array.Empty<XboxApp>();
        private XboxLiveAccount? _xboxLiveAccount;

        internal SmartGlassClient? Client { get; private set; }

        public override string Title => "Microsoft Xbox (" + IpAddress + ")";

        public string IconName => "fas fa-gamepad";

        [State]
        public bool IsConnected => _xboxLiveAccount != null && Client != null;

        [State]
        public IActivityDevice.DeviceActivity Activity { get; private set; }

        [State]
        public IActivityDevice.DeviceActivity[] Activities { get; private set; } = Array.Empty<IActivityDevice.DeviceActivity>();

        [Configuration]
        public string? IpAddress { get; set; }

        [Configuration]
        public string? XboxLiveAccountId { get; set; }

        [Configuration]
        public string? DeviceId { get; set; }

        protected override TimeSpan PollingInterval => TimeSpan.FromSeconds(5);

        protected override TimeSpan FailureInterval => TimeSpan.FromSeconds(10);

        public XboxDevice(IThingManager thingManager, ILogger<XboxDevice> logger)
            : base(logger)
        {
            _thingManager = thingManager;
            _logger = logger;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (Client == null && IpAddress != null)
                {
                    _xboxLiveAccount = _thingManager.TryGetById(XboxLiveAccountId) as XboxLiveAccount;

                    if (_xboxLiveAccount?.IsAuthenticated == true && _xboxLiveAccount?.Authentication != null)
                    {
                        try
                        {
                            Client = await SmartGlassClient.ConnectAsync(IpAddress,
                                _xboxLiveAccount.Authentication.UserInformation.Userhash,
                                _xboxLiveAccount.Authentication.XToken.Jwt);
                        }
                        catch
                        {
                            Client = await SmartGlassClient.ConnectAsync(IpAddress,
                                _xboxLiveAccount.Authentication.UserInformation.Userhash,
                                _xboxLiveAccount.Authentication.XToken.Jwt);
                        }

                        Client.ConsoleStatusChanged += OnConsoleStatusChanged;
                        Client.ProtocolTimeoutOccured += OnProtocolTimeoutOccured;

                        await RefreshAsync(cancellationToken);

                        Update(Client.CurrentConsoleStatus);
                    }
                }
            }
            catch (TimeoutException)
            {
                // Xbox is offline.
                Activity = default;
                Activities = Array.Empty<IActivityDevice.DeviceActivity>();
            }
        }

        [Operation]
        public async Task RefreshAsync(CancellationToken cancellationToken)
        {
            if (IsConnected && _xboxLiveAccount?.Authentication?.XToken != null && DeviceId != null)
            {
                var config = new XblConfiguration(_xboxLiveAccount.Authentication.XToken, XblLanguage.United_States);
                var service = new SmartGlassService(config, _logger);

                _installedApps = await service.GetInstalledAppsJsonAsync(DeviceId, cancellationToken);
                Activities = _installedApps
                    .OrderBy(a => a.Name)
                    .Select(a => new IActivityDevice.DeviceActivity { Id = a.OneStoreProductId, Title = a.Name })
                    .ToArray();
            }
            else
            {
                Activity = default;
                Activities = Array.Empty<IActivityDevice.DeviceActivity>();
            }

            _thingManager.DetectChanges(this);
        }

        [Operation]
        public async Task ChangeActivityAsync(string activityId, CancellationToken cancellationToken)
        {
            if (IsConnected && _xboxLiveAccount?.Authentication?.XToken != null && DeviceId != null)
            {
                var config = new XblConfiguration(_xboxLiveAccount.Authentication.XToken, XblLanguage.United_States);
                var service = new SmartGlassService(config, _logger);
                await service.LaunchAppAsync(DeviceId, activityId, cancellationToken);
            }
        }

        private void OnConsoleStatusChanged(object? sender, ConsoleStatusChangedEventArgs e)
        {
            Update(e.Status);
        }

        private void OnProtocolTimeoutOccured(object? sender, EventArgs e)
        {
            Client?.Dispose();
            Client = null;

            Activity = default;
            Activities = Array.Empty<IActivityDevice.DeviceActivity>();

            _thingManager.DetectChanges(this);
        }

        private void Update(ConsoleStatus consoleStatus)
        {
            var focusTitle = consoleStatus.ActiveTitles.FirstOrDefault(a => a.HasFocus);
            var app = _installedApps.SingleOrDefault(a => a.Aumid == focusTitle?.AumId);

            Activity = new IActivityDevice.DeviceActivity
            {
                Id = app?.OneStoreProductId ?? "n/a",
                Title = app?.Name ?? focusTitle?.AumId ?? string.Empty,
            };

            _thingManager.DetectChanges(this);
        }

        public override void Dispose()
        {
            Client?.Dispose();
            base.Dispose();
        }
    }
}
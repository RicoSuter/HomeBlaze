using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Inputs;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Zwave.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ZWave;
using ZWave.Channel;
using ZWave.CommandClasses;

namespace HomeBlaze.Zwave
{
    public class ZwaveDevice : IThing, IIconProvider
    {
        private bool _loadingMetadata = false;

        public Node Node { get; private set; }

        public NodeProtocolInfo Info { get; private set; }

        public ZwaveController Controller { get; private set; }

        public byte NodeId => Node.NodeID;

        public string Id => Controller.Id + $"/nodes/{NodeId}";

        public string? Title => DeviceDescription != null ?
            $"{Manufacturer} / {Description} ({Label}) (Node {NodeId})" :
            $"{GenericType} / {BasicType} (Node {NodeId})";

        public string IconName => "fa-regular fa-hard-drive";

        public DateTimeOffset? LastBatteryUpdated { get; private set; }

        private ZwaveBatteryComponent? _batteryComponent;
        private ZwaveCentralSceneComponent? _buttonComponent;

        private ZwaveNotificationComponent? _notificationComponent;
        private ZwaveSensorAlarmComponent? _sensorAlarmComponent;
        private ZwavePowerConsumptionComponent? _powerConsumptionComponent;
        private ZwaveDoorSensorComponent? _doorSensorComponent;

        private List<ZwaveClassComponent> _multiChannelComponents = new();
        private List<ZwaveSensorComponent> _sensorComponents = new();

        internal VersionCommandClassReport[]? VersionCommandClasses { get; private set; }

        internal ManufacturerSpecificReport? ManufacturerInfo { get; private set; }

        internal AssociationGroupsReport? AssociationGroupsReport { get; private set; }

        internal MultiChannelEndPointReport? MultiChannelEndPointReport { get; private set; }

        internal ZwaveDeviceDescription? DeviceDescription { get; private set; }

        [State]
        public IThing[] Things { get; private set; } = Array.Empty<IThing>();

        [State]
        public string? Manufacturer => DeviceDescription?.Manufacturer;

        [State]
        public string? Label => DeviceDescription?.Label;

        [State]
        public string? Description => DeviceDescription?.Description;

        [State]
        public string? BasicType => Info?.BasicType.ToString() ?? "n/a";

        [State]
        public string? GenericType => Info?.GenericType.ToString() ?? "n/a";

        [State]
        public string? SpecificType => Info?.SpecificType.ToString() ?? "n/a";

        [State]
        public bool? IsListening => Info?.IsListening;

        [State]
        public string? Version => Info?.Version.ToString();

        [State]
        public int? ProductType => ManufacturerInfo?.ProductType;

        [State]
        public int? ManufacturerId => ManufacturerInfo?.ManufacturerID;

        [State]
        public int? ProductId => ManufacturerInfo?.ProductID;

        [State]
        public DateTimeOffset? LastUpdated => Things?
            .OfType<ZwaveClassComponent>()
            .Select(c => c.LastUpdated)
            .Max();

        [State]
        public string[]? CommandClasses { get; private set; }

        [State]
        public string? InclusionDescription => DeviceDescription?.Metadata?.Inclusion;

        [State]
        public string? ExclusionDescription => DeviceDescription?.Metadata?.Exclusion;

        [State]
        public string? ResetDescription => DeviceDescription?.Metadata?.Reset;

        [State]
        public string? ManualUrl => DeviceDescription?.Metadata?.Manual;

        public ZwaveDevice(Node node, NodeProtocolInfo info, ZwaveController controller)
        {
            Controller = controller;
            Node = node;
            Info = info;

            Update(node, info);
        }

        internal virtual ZwaveDevice Update(Node node, NodeProtocolInfo info)
        {
            Node = node;
            Info = info;

            return this;
        }

        [Operation]
        public async Task RefreshAsync(CancellationToken cancellationToken)
        {
            if (!_loadingMetadata)
            {
                _loadingMetadata = true;
                try
                {
                    await Retry.RetryAsync(async () =>
                    {
                        await TryRefreshManufacturerInfoAsync(cancellationToken);
                        await TryRefreshVersionCommandClassesAsync(cancellationToken);
                        await TryRefreshBatteryAsync(cancellationToken);
                        await TryRefreshMultiChannelAsync(cancellationToken);
                        await TryRefreshMultiChannelAssociationAsync(cancellationToken);
                        await TryRefreshSensorMultiLevelAsync(cancellationToken);
                        await TryRefreshSensorBinaryAsync(cancellationToken);
                    }, Controller.Logger);
                }
                catch (Exception e)
                {
                    Controller.Logger.LogWarning(e, "Failed to refresh Z-Wave device.");
                }
                finally
                {
                    UpdateThings();
                    _loadingMetadata = false;
                }
            }
        }

        [Operation]
        public async Task<string> HealNodeNetworkAsync(CancellationToken cancellationToken)
        {
            return (await Node.HealNodeNetwork(cancellationToken)).ToString();
        }

        [Operation]
        public async Task RemoveFailedNode(CancellationToken cancellationToken)
        {
            await Node.RemoveFailedNode(cancellationToken);
        }

        private async Task TryRefreshSensorBinaryAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (VersionCommandClasses != null &&
                    VersionCommandClasses.Any(c => c.Class == CommandClass.SensorBinary))
                {
                    var sensorBinary = Node.GetCommandClass<SensorBinary>();
                    await sensorBinary.Get(cancellationToken);
                }
            }
            catch { }
        }

        private async Task TryRefreshSensorMultiLevelAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (VersionCommandClasses != null &&
                    VersionCommandClasses.Any(c => c.Class == CommandClass.SensorMultiLevel))
                {
                    var sensorMultiLevel = Node.GetCommandClass<SensorMultiLevel>();
                    var report = await sensorMultiLevel.GetSupportedSensors(cancellationToken);
                    foreach (var sensorType in report.SupportedSensorTypes)
                    {
                        var scaleReport = await sensorMultiLevel.GetScale(sensorType, cancellationToken);
                        foreach (var scale in scaleReport.SupportedScales)
                        {
                            await sensorMultiLevel.Get(sensorType, Convert.ToByte(scale), cancellationToken);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Controller.Logger.LogWarning(e, "Failed to TryRefreshSensorMultiLevelAsync.");
            }
        }

        private async Task TryRefreshMultiChannelAssociationAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (AssociationGroupsReport == null &&
                    VersionCommandClasses != null &&
                    VersionCommandClasses.Any(c => c.Class == CommandClass.MultiChannelAssociation))
                {
                    var multiChannelAssociation = Node.GetCommandClass<MultiChannelAssociation>();
                    AssociationGroupsReport = await multiChannelAssociation.GetGroups(cancellationToken);
                }
            }
            catch { }
        }

        private async Task TryRefreshMultiChannelAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (MultiChannelEndPointReport == null &&
                    VersionCommandClasses != null &&
                    VersionCommandClasses.Any(c => c.Class == CommandClass.MultiChannel))
                {
                    var multiChannel = Node.GetCommandClass<MultiChannel>();
                    MultiChannelEndPointReport = await multiChannel.DiscoverEndpoints(cancellationToken);

                    if (MultiChannelEndPointReport != null)
                    {
                        for (byte index = 1; index <= MultiChannelEndPointReport.NumberOfIndividualEndPoints; index++)
                        {
                            var capability = await multiChannel.GetEndPointCapabilities(index, cancellationToken);
                            if (capability.SupportedCommandClasses.Contains(CommandClass.SwitchBinary))
                            {
                                var switchBinary = multiChannel.GetEndPointCommandClass<SwitchBinary>(index);
                                var report = await switchBinary.Get(cancellationToken);

                                var component = _multiChannelComponents
                                    .OfType<ZwaveSwitchComponent>()
                                    .SingleOrDefault(c => c.EndPointId == index);

                                if (component == null)
                                {
                                    component = new ZwaveSwitchComponent(this, switchBinary, index);
                                    switchBinary.Changed += component.OnSwitchBinary;
                                    _multiChannelComponents.Add(component);
                                }

                                component.OnSwitchBinary(Node, new ReportEventArgs<SwitchBinaryReport>(report));
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private async Task TryRefreshBatteryAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (VersionCommandClasses != null &&
                    VersionCommandClasses.Any(c => c.Class == CommandClass.Battery))
                {
                    // Update every 6 hours
                    if (_batteryComponent == null ||
                        LastBatteryUpdated == null ||
                        DateTimeOffset.Now - LastBatteryUpdated > TimeSpan.FromHours(6))
                    {
                        var battery = Node.GetCommandClass<Battery>();
                        var batteryReport = await battery.Get(cancellationToken);

                        LastBatteryUpdated = DateTimeOffset.Now;
                        OnBattery(Node, new ReportEventArgs<BatteryReport>(batteryReport));
                    }
                }
            }
            catch { }
        }

        private async Task TryRefreshVersionCommandClassesAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (VersionCommandClasses == null)
                {
                    VersionCommandClasses = await Node.GetSupportedCommandClasses(cancellationToken);
                    CommandClasses = VersionCommandClasses?.Select(c => c.Class.ToString()).ToArray();
                }
            }
            catch { }
        }

        private async Task TryRefreshManufacturerInfoAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (ManufacturerInfo == null)
                {
                    var manufacturerSpecific = Node.GetCommandClass<ManufacturerSpecific>();
                    ManufacturerInfo = await manufacturerSpecific.Get(cancellationToken);

                    var productId = "0x" + ManufacturerInfo.ProductID.ToString("x4");
                    var productType = "0x" + ManufacturerInfo.ProductType.ToString("x4");
                    var manufacturerId = "0x" + ManufacturerInfo.ManufacturerID.ToString("x4");

                    var basePath = "HomeBlaze.Zwave.Devices._" + manufacturerId;
                    var assembly = typeof(ZwaveDevice).Assembly;

                    var options = new JsonSerializerOptions
                    {
                        ReadCommentHandling = JsonCommentHandling.Skip
                    };

                    DeviceDescription = assembly?
                        .GetManifestResourceNames()
                        .Where(n => n.StartsWith(basePath))
                        .Select(n =>
                        {
                            try
                            {
                                using (var stream = assembly!.GetManifestResourceStream(n))
                                {
                                    if (stream != null)
                                    {
                                        using (var reader = new StreamReader(stream))
                                        {
                                            var json = reader.ReadToEnd();
                                            return JsonSerializer.Deserialize<ZwaveDeviceDescription>(json, options);
                                        }
                                    }
                                }

                                return null;
                            }
                            catch
                            {
                                return null;
                            }
                        })
                        .FirstOrDefault(d => d?.Devices?
                            .Any(d => d.ProductId == productId &&
                                      d.ProductType == productType) == true);

                    // Details can be retrieved here: 
                    // https://github.com/zwave-js/node-zwave-js/tree/master/packages/config/config/devices

                    // Manuals (structured)
                    // https://github.com/OpenZWave/open-zwave/tree/master/config

                    // TODO(zwave): Also use files from https://github.com/zwave-js/node-zwave-js/tree/master/packages/config/config
                }
            }
            catch { }
        }

        internal async void OnWakeUp(object? sender, ReportEventArgs<WakeUpReport> e)
        {
            await RefreshAsync(CancellationToken.None);
        }

        internal void OnMessageReceived(object? sender, EventArgs e)
        {
            if (e is NodeEventArgs nodeEventArgs)
            {
                Controller.Logger.LogInformation("Z-Wave message with class ID {ClassId} and command ID {CommandId} received for node {NodeId}.", nodeEventArgs.Command.ClassID, nodeEventArgs.Command.CommandID, nodeEventArgs.NodeID);
            }
        }

        internal void OnUpdateReceived(object? sender, EventArgs e)
        {
            if (e is NodeEventArgs nodeEventArgs)
            {
                Controller.Logger.LogInformation("Z-Wave update with class ID {ClassId} and command ID {CommandId} received for node {NodeId}.", nodeEventArgs.Command.ClassID, nodeEventArgs.Command.CommandID, nodeEventArgs.NodeID);
            }
        }

        internal void OnUnknownCommandReceived(object? sender, NodeEventArgs nodeEventArgs)
        {
            Controller.Logger.LogInformation("Unknown Z-Wave message with class ID {ClassId} and command ID {CommandId} received for node {NodeId}.", nodeEventArgs.Command.ClassID, nodeEventArgs.Command.CommandID, nodeEventArgs.NodeID);
        }

        internal void OnMeter(object? sender, ReportEventArgs<MeterReport> e)
        {
            if (e.Report.Type == MeterType.Electric)
            {
                _powerConsumptionComponent ??= new ZwavePowerConsumptionComponent(this);
                _powerConsumptionComponent.Value = (decimal)e.Report.Value;
                _powerConsumptionComponent.Unit = e.Report.Unit;
                _powerConsumptionComponent.Scale = (ElectricMeterScale)e.Report.Scale;

                Controller.ThingManager.DetectChanges(_powerConsumptionComponent);
            }
            else
            {
                // TODO: Implement other meters (gas, etc.)
            }
        }

        internal void OnSwitchBinary(object? sender, ReportEventArgs<SwitchBinaryReport> e)
        {
            Controller.Logger.LogInformation("OnSwitchMultiLevel: Z-Wave switch binary value {Value} received for node {NodeId}.", e.Report.CurrentValue, e.Report?.Node?.NodeID);
        }

        internal void OnSwitchMultiLevel(object? sender, ReportEventArgs<SwitchMultiLevelReport> e)
        {
            Controller.Logger.LogInformation("OnSwitchMultiLevel: Z-Wave switch multi level value {Value} received for node {NodeId}.", e.Report.CurrentValue, e.Report?.Node?.NodeID);
        }

        internal void OnSensorMultiLevel(object? sender, ReportEventArgs<SensorMultiLevelReport> e)
        {
            var component = _sensorComponents.SingleOrDefault(c => c.SensorType == e.Report.Type);
            if (component == null)
            {
                if (e.Report.Type == SensorType.Temperature)
                {
                    _sensorComponents.Add(component = new ZwaveTemperatureSensorComponent(this));
                }
                else if (e.Report.Type == SensorType.Luminance)
                {
                    _sensorComponents.Add(component = new ZwaveLuminanceSensorComponent(this));
                }
                else if (e.Report.Type == SensorType.RelativeHumidity)
                {
                    _sensorComponents.Add(component = new ZwaveRelativeHumiditySensorComponent(this));
                }
                else if (e.Report.Type == SensorType.RainRate)
                {
                    _sensorComponents.Add(component = new ZwaveRainSensorComponent(this));
                }
                else
                {
                    _sensorComponents.Add(component = new ZwaveSensorComponent(this, e.Report.Type));
                }
            }

            component.Value = (decimal)e.Report.Value;
            component.Scale = e.Report.Scale;
            component.Unit = e.Report.Unit;
            component.LastUpdated = DateTimeOffset.Now;

            Controller.ThingManager.DetectChanges(component);

            UpdateThings();
        }

        internal void OnSensorAlarm(object? sender, ReportEventArgs<SensorAlarmReport> e)
        {
            _sensorAlarmComponent ??= new ZwaveSensorAlarmComponent(this);
            _sensorAlarmComponent.Level = e.Report.Level;
            _sensorAlarmComponent.Source = e.Report.Source;
            _sensorAlarmComponent.NotificationType = e.Report.Type;

            _sensorAlarmComponent.LastUpdated = DateTimeOffset.Now;

            Controller.ThingManager.DetectChanges(_sensorAlarmComponent);
            UpdateThings();
        }

        internal void OnBasic(object? sender, ReportEventArgs<BasicReport> e)
        {
            Controller.Logger.LogInformation("OnBasic: Z-Wave basic value {Value} received for node {NodeId}.", e.Report.CurrentValue, e.Report?.Node?.NodeID);
        }

        internal void OnSensorBinary(object? sender, ReportEventArgs<SensorBinaryReport> e)
        {
            Controller.Logger.LogInformation("OnSensorBinary: Z-Wave sensor binary value {Value} received for node {NodeId}.", e.Report.Value, e.Report?.Node?.NodeID);
        }

        internal void OnNotification(object? sender, ReportEventArgs<AlarmReport> e)
        {
            _notificationComponent ??= new ZwaveNotificationComponent(this);
            _notificationComponent.Level = e.Report.Level;
            _notificationComponent.Status = e.Report.Status;
            _notificationComponent.Event = e.Report.Event;

            _notificationComponent.LastUpdated = DateTimeOffset.Now;

            // TODO: Extend enum and implement other sensors

            if (_notificationComponent.Event == NotificationState.WindowDoorOpen ||
                _notificationComponent.Event == NotificationState.WindowDoorClosed)
            {
                _doorSensorComponent ??= new ZwaveDoorSensorComponent(_notificationComponent);

                Controller.ThingManager.DetectChanges(_doorSensorComponent);
            }

            Controller.ThingManager.DetectChanges(_notificationComponent);
            UpdateThings();
        }

        internal void OnBattery(object? sender, ReportEventArgs<BatteryReport> e)
        {
            _batteryComponent ??= new ZwaveBatteryComponent(this);
            _batteryComponent.BatteryLevel = e.Report?.IsLow == true ? 0.1m : e.Report?.Value / 100m; // 255 is used for IsLow
            _batteryComponent.LastUpdated = DateTimeOffset.Now;

            Controller.ThingManager.DetectChanges(_batteryComponent);
            UpdateThings();
        }

        internal async void OnCentralScene(object? sender, ReportEventArgs<CentralSceneReport> e)
        {
            _buttonComponent = _buttonComponent ?? new ZwaveCentralSceneComponent(this);

            if (e.Report.KeyState == CentralSceneKeyState.KeyPressed)
            {
                _buttonComponent.ButtonState = ButtonState.Press;
                Controller.ThingManager.DetectChanges(_buttonComponent);

                await Task.Delay(100);

                _buttonComponent.ButtonState = ButtonState.None;
                Controller.ThingManager.DetectChanges(_buttonComponent);
            }
            else if (e.Report.KeyState == CentralSceneKeyState.KeyHeldDown)
            {
                _buttonComponent.ButtonState = ButtonState.Down;
                Controller.ThingManager.DetectChanges(_buttonComponent);
            }
            else if (e.Report.KeyState == CentralSceneKeyState.KeyReleased)
            {
                _buttonComponent.ButtonState = ButtonState.LongPress;
                Controller.ThingManager.DetectChanges(_buttonComponent);

                await Task.Delay(100);

                _buttonComponent.ButtonState = ButtonState.None;
                Controller.ThingManager.DetectChanges(_buttonComponent);
            }

            _buttonComponent.LastUpdated = DateTimeOffset.Now;
            Controller.ThingManager.DetectChanges(_buttonComponent);

            UpdateThings();
        }

        private void UpdateThings()
        {
            var things = new IThing?[]
            {
                _batteryComponent,
                _buttonComponent,
                _notificationComponent,
                _sensorAlarmComponent,
                _powerConsumptionComponent,
                _doorSensorComponent
            };

            Things = things
                .OfType<IThing>()
                .Concat(_multiChannelComponents)
                .Concat(_sensorComponents)
                .ToArray()!;

            Controller.ThingManager.DetectChanges(this);
        }
    }
}

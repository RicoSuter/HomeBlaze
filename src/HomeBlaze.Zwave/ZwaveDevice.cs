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
        public Node Node { get; private set; }

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

        private ZwaveSensorAlarmComponent? _sensorAlarmComponent;
        private ZwavePowerConsumptionComponent? _powerConsumptionComponent;

        private List<ZwaveClassComponent> _multiChannelComponents = new();
        private List<ZwaveSensorComponent> _sensorComponents = new();

        internal NodeProtocolInfo? Info { get; set; }

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
        public DateTimeOffset? LastMessageReceived { get; private set; }

        [State]
        public string? InclusionDescription => DeviceDescription?.Metadata?.Inclusion;

        [State]
        public string? ExclusionDescription => DeviceDescription?.Metadata?.Exclusion;

        [State]
        public string? ResetDescription => DeviceDescription?.Metadata?.Reset;

        [State]
        public string? ManualUrl => DeviceDescription?.Metadata?.Manual;

        //[State]
        //public bool IsLoading { get; set; } = false;

        public ZwaveDevice(Node node, ZwaveController controller)
        {
            Controller = controller;
            Node = node;
        }

        //[Operation]
        //public async Task RefreshAsync(CancellationToken cancellationToken)
        //{
        //    if (!IsLoading &&
        //        Info is not null &&
        //        Info.BasicType != ZWave.BasicType.StaticController)
        //    {
        //        IsLoading = true;
        //        Controller.ThingManager.DetectChanges(this);

        //        try
        //        {
        //            var tasks = new List<Task>
        //            {
        //                Task.Run(() => TryRefreshMultiChannelAsync(cancellationToken), cancellationToken),
        //                Task.Run(() => TryRefreshManufacturerInfoAsync(cancellationToken), cancellationToken),
        //                Task.Run(() => TryRefreshBatteryAsync(cancellationToken), cancellationToken),
        //                Task.Run(() => TryRefreshMultiChannelAssociationAsync(cancellationToken), cancellationToken),
        //                Task.Run(() => TryRefreshSensorMultiLevelAsync(cancellationToken), cancellationToken),
        //                Task.Run(() => TryRefreshSensorBinaryAsync(cancellationToken), cancellationToken),
        //            };

        //            await Task.WhenAll(tasks);
        //        }
        //        catch (Exception e)
        //        {
        //            Controller.Logger.LogWarning(e, "Failed to refresh Z-Wave device.");
        //        }
        //        finally
        //        {
        //            IsLoading = false;
        //        }

        //        UpdateThings();
        //    }
        //}

        [Operation]
        public async Task SetConfigurationAsync(byte parameter, sbyte value, CancellationToken cancellationToken)
        {
            var configuration = Node.GetCommandClass<Configuration>();
            await configuration.Set(parameter, value, cancellationToken);
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

        internal async Task TryRefreshInfoAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (Info == null)
                {
                    Info = await Node.GetProtocolInfo(cancellationToken);
                }
            }
            catch (Exception e)
            {
                Controller.Logger.LogWarning(e, "Failed to TryRefreshInfo for node {NodeId}.", NodeId);
            }

            UpdateThings();
        }

        internal async Task TryRefreshManufacturerInfoAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (ManufacturerInfo == null && 
                    Info is not null && 
                    Info.BasicType != ZWave.BasicType.StaticController)
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
            catch (Exception e)
            {
                Controller.Logger.LogWarning(e, "Failed to TryRefreshManufacturerInfoAsync for node {NodeId}.", NodeId);
            }

            UpdateThings();
        }

        //private async Task TryRefreshSensorBinaryAsync(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (VersionCommandClasses != null &&
        //            VersionCommandClasses.Any(c => c.Class == CommandClass.SensorBinary))
        //        {
        //            var sensorBinary = Node.GetCommandClass<SensorBinary>();
        //            await sensorBinary.Get(cancellationToken);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Controller.Logger.LogWarning(e, "Failed to TryRefreshSensorBinaryAsync.");
        //    }

        //    UpdateThings();
        //}

        //private async Task TryRefreshSensorMultiLevelAsync(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (VersionCommandClasses != null &&
        //            VersionCommandClasses.Any(c => c.Class == CommandClass.SensorMultiLevel))
        //        {
        //            var sensorMultiLevel = Node.GetCommandClass<SensorMultiLevel>();
        //            var report = await sensorMultiLevel.GetSupportedSensors(cancellationToken);
        //            foreach (var sensorType in report.SupportedSensorTypes)
        //            {
        //                var scaleReport = await sensorMultiLevel.GetScale(sensorType, cancellationToken);
        //                foreach (var scale in scaleReport.SupportedScales)
        //                {
        //                    await sensorMultiLevel.Get(sensorType, Convert.ToByte(scale), cancellationToken);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Controller.Logger.LogWarning(e, "Failed to TryRefreshSensorMultiLevelAsync.");
        //    }

        //    UpdateThings();
        //}

        //private async Task TryRefreshMultiChannelAssociationAsync(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (AssociationGroupsReport == null &&
        //            VersionCommandClasses != null &&
        //            VersionCommandClasses.Any(c => c.Class == CommandClass.MultiChannelAssociation))
        //        {
        //            var multiChannelAssociation = Node.GetCommandClass<MultiChannelAssociation>();
        //            AssociationGroupsReport = await multiChannelAssociation.GetGroups(cancellationToken);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Controller.Logger.LogWarning(e, "Failed to TryRefreshMultiChannelAssociationAsync.");
        //    }
        //    UpdateThings();
        //}

        //private async Task TryRefreshMultiChannelAsync(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (MultiChannelEndPointReport == null &&
        //            VersionCommandClasses != null &&
        //            VersionCommandClasses.Any(c => c.Class == CommandClass.MultiChannel))
        //        {
        //            var multiChannel = Node.GetCommandClass<MultiChannel>();
        //            MultiChannelEndPointReport = await multiChannel.DiscoverEndpoints(cancellationToken);

        //            if (MultiChannelEndPointReport != null)
        //            {
        //                for (byte index = 1; index <= MultiChannelEndPointReport.NumberOfIndividualEndPoints; index++)
        //                {
        //                    var capability = await multiChannel.GetEndPointCapabilities(index, cancellationToken);
        //                    if (capability.SupportedCommandClasses.Contains(CommandClass.SwitchBinary))
        //                    {
        //                        var switchBinary = multiChannel.GetEndPointCommandClass<SwitchBinary>(index);
        //                        var report = await switchBinary.Get(cancellationToken);

        //                        var component = _multiChannelComponents
        //                            .OfType<ZwaveSwitchComponent>()
        //                            .SingleOrDefault(c => c.EndPointId == index);

        //                        if (component == null)
        //                        {
        //                            component = new ZwaveSwitchComponent(this, switchBinary, index);
        //                            switchBinary.Changed += component.OnSwitchBinary;
        //                            _multiChannelComponents.Add(component);
        //                        }

        //                        component.OnSwitchBinary(Node, new ReportEventArgs<SwitchBinaryReport>(report));
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Controller.Logger.LogWarning(e, "Failed to TryRefreshMultiChannelAsync.");
        //    }
        //    UpdateThings();
        //}

        //private async Task TryRefreshBatteryAsync(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        if (VersionCommandClasses != null &&
        //            VersionCommandClasses.Any(c => c.Class == CommandClass.Battery))
        //        {
        //            // Update every 6 hours
        //            if (_batteryComponent == null ||
        //                LastBatteryUpdated == null ||
        //                DateTimeOffset.Now - LastBatteryUpdated > TimeSpan.FromHours(6))
        //            {
        //                var battery = Node.GetCommandClass<Battery>();
        //                var batteryReport = await battery.Get(cancellationToken);

        //                LastBatteryUpdated = DateTimeOffset.Now;
        //                OnBattery(Node, new ReportEventArgs<BatteryReport>(batteryReport));
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Controller.Logger.LogWarning(e, "Failed to TryRefreshBatteryAsync.");
        //    }

        //    UpdateThings();
        //}

        internal async void OnWakeUp(object? sender, ReportEventArgs<WakeUpReport> e)
        {
            Controller.Logger.LogInformation("Z-Wave node {NodeId} woke up.", NodeId);
            await TryRefreshManufacturerInfoAsync(CancellationToken.None);
        }

        internal void OnMessageReceived(object? sender, EventArgs e)
        {
            LastMessageReceived = DateTimeOffset.Now;

            if (e is NodeEventArgs nodeEventArgs)
            {
                Controller.Logger.LogInformation("Z-Wave message with class ID {ClassId} ({Class}) and command ID {CommandId} received for node {NodeId}.",
                    nodeEventArgs.Command.ClassID, (CommandClass)nodeEventArgs.Command.ClassID, nodeEventArgs.Command.CommandID, nodeEventArgs.NodeID);
            }
        }

        internal void OnUpdateReceived(object? sender, EventArgs e)
        {
            if (e is NodeEventArgs nodeEventArgs)
            {
                Controller.Logger.LogInformation("Z-Wave update with class ID {ClassId} ({Class}) and command ID {CommandId} received for node {NodeId}.",
                    nodeEventArgs.Command.ClassID, (CommandClass)nodeEventArgs.Command.ClassID, nodeEventArgs.Command.CommandID, nodeEventArgs.NodeID);
            }
        }

        internal void OnUnknownCommandReceived(object? sender, NodeEventArgs nodeEventArgs)
        {
            Controller.Logger.LogInformation("Unknown Z-Wave message with class ID {ClassId} ({Class}) and command ID {CommandId} received for node {NodeId}.",
                nodeEventArgs.Command.ClassID, (CommandClass)nodeEventArgs.Command.ClassID, nodeEventArgs.Command.CommandID, nodeEventArgs.NodeID);
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
            OnMultiChannelSwitchBinary(sender, e, 0);
        }

        internal void OnMultiChannelSwitchBinary(object? sender, ReportEventArgs<SwitchBinaryReport> e, byte endPointID)
        {
            if (e.Report != null)
            {
                var component = _multiChannelComponents
                    .OfType<ZwaveSwitchComponent>()
                    .SingleOrDefault(c => c.EndPointId == endPointID);

                if (component == null)
                {
                    component = new ZwaveSwitchComponent(this,
                        endPointID == 0 ?
                            Node.GetCommandClass<SwitchBinary>() :
                            Node.GetCommandClass<MultiChannel>().GetEndPointCommandClass<SwitchBinary>(endPointID),
                        endPointID);

                    _multiChannelComponents.Add(component);
                }

                component.OnSwitchBinary(Node, new ReportEventArgs<SwitchBinaryReport>(e.Report));
            }
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

        internal void OnMultiChannel(object? sender, ReportEventArgs<MultiChannelReport> e)
        {
            if (e.Report.Report is AlarmReport alarmReport)
            {
                OnMultiChannelAlarm(sender, new ReportEventArgs<AlarmReport>(alarmReport), e.Report.EndPointID);
            }
            else if (e.Report.Report is SwitchBinaryReport switchBinaryReport)
            {
                OnMultiChannelSwitchBinary(sender, new ReportEventArgs<SwitchBinaryReport>(switchBinaryReport), e.Report.EndPointID);
            }

            Controller.Logger.LogInformation("OnMultiChannel: Z-Wave multi channel report of type {Type} received for node {NodeId}.", e.Report.Report?.GetType().FullName, e.Report?.Node?.NodeID);
        }

        internal void OnNotification(object? sender, ReportEventArgs<NotificationReport> e)
        {
            Controller.Logger.LogInformation("OnNotification: Z-Wave notification event {Event} received for node {NodeId}.", e.Report.Event, e.Report?.Node?.NodeID);
        }

        internal void OnAlarm(object? sender, ReportEventArgs<AlarmReport> e)
        {
            OnMultiChannelAlarm(sender, e, 0);
        }

        internal void OnMultiChannelAlarm(object? sender, ReportEventArgs<AlarmReport> e, byte endPointID)
        {
            if (e.Report != null)
            {
                var component = _multiChannelComponents
                    .OfType<ZwaveNotificationComponent>()
                    .SingleOrDefault(c => c.EndPointId == endPointID);

                if (component == null)
                {
                    component = new ZwaveNotificationComponent(this, endPointID);
                    _multiChannelComponents.Add(component);
                }

                component.Level = e.Report.Level;
                component.Status = e.Report.Status;
                component.Event = e.Report.Event;
                component.Type = e.Report.Type;
                component.LastUpdated = DateTimeOffset.Now;

                if (component.Event == NotificationState.WindowDoorOpen ||
                    component.Event == NotificationState.WindowDoorClosed)
                {
                    var doorSensorComponent = _multiChannelComponents
                        .OfType<ZwaveDoorSensorComponent>()
                        .SingleOrDefault(c => c.EndPointId == endPointID);

                    if (doorSensorComponent == null)
                    {
                        doorSensorComponent = new ZwaveDoorSensorComponent(component, endPointID);
                        _multiChannelComponents.Add(doorSensorComponent);
                    }

                    Controller.ThingManager.DetectChanges(doorSensorComponent);
                }

                if (component.Type == NotificationType.Flood)
                {
                    var floodSensorComponent = _multiChannelComponents
                       .OfType<ZwaveFloodSensorComponent>()
                       .SingleOrDefault(c => c.EndPointId == endPointID);

                    if (floodSensorComponent == null)
                    {
                        floodSensorComponent = new ZwaveFloodSensorComponent(component, endPointID);
                        _multiChannelComponents.Add(floodSensorComponent);
                    }

                    floodSensorComponent.IsFlooding =
                        component.Event == NotificationState.LeakDetected ||
                        component.Event == NotificationState.LeakDetectedUnknownLocation;

                    Controller.ThingManager.DetectChanges(floodSensorComponent);
                }

                Controller.ThingManager.DetectChanges(component);
                UpdateThings();
            }
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
                _sensorAlarmComponent,
                _powerConsumptionComponent,
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

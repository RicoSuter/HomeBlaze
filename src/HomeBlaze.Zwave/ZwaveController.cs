using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZWave;
using ZWave.CommandClasses;

namespace HomeBlaze.Zwave
{
    [DisplayName("Z-Wave Controller")]
    [ThingSetup(typeof(ZwaveControllerSetup), CanEdit = true)]
    public class ZwaveController : PollingThing, IIconProvider, IConnectedThing
    {
        private ZWaveController? _controller;

        internal ILogger Logger { get; }

        public string IconName => "fab fa-hubspot";

        public override string? Title => $"Z-Wave Controller ({ActualSerialPort ?? "n/a"})";

        [State]
        public IThing[] Things { get; private set; } = Array.Empty<IThing>();

        [State]
        public bool IsConnected { get; private set; }

        [State]
        public string? Version { get; private set; }

        [State]
        public bool IsAddingNodes { get; private set; }

        [State]
        public bool IsRemovingNodes { get; private set; }

        [State]
        public bool IsRefreshingNodes { get; private set; } = false;

        [State]
        public bool IsRefreshingMetadata { get; private set; } = false;

        [Configuration]
        public string? SerialPort { get; set; } = "COM7;/dev/ttyACM1";

        private string? ActualSerialPort => "COM7";

        protected override TimeSpan PollingInterval => TimeSpan.FromSeconds(60);

        protected override TimeSpan FailureInterval => TimeSpan.FromSeconds(60);

        public ZwaveController(IThingManager thingManager, ILogger<ZWaveController> logger)
            : base(thingManager, logger)
        {
            Logger = logger;
        }

        [Operation]
        public async Task<bool> StartAddingNodesToNetworkAsync(CancellationToken cancellationToken)
        {
            if (_controller != null && !IsAddingNodes && !IsRemovingNodes)
            {
                if (await _controller.StartAddingNodesToNetwork(cancellationToken))
                {
                    IsAddingNodes = true;
                    ThingManager.DetectChanges(this);
                    return true;
                }
            }
            return false;
        }

        [Operation]
        public async Task<bool> StopAddingNodesToNetworkAsync(CancellationToken cancellationToken)
        {
            if (_controller != null && IsAddingNodes && !IsRemovingNodes)
            {
                if (await _controller.StopAddingNodesToNetwork(cancellationToken))
                {
                    IsAddingNodes = false;
                    ThingManager.DetectChanges(this);
                    return true;
                }
            }
            return false;
        }

        [Operation]
        public async Task<bool> StartRemoveNodeFromNetworkAsync(CancellationToken cancellationToken)
        {
            if (_controller != null && !IsAddingNodes && !IsRemovingNodes)
            {
                if (await _controller.StartRemoveNodeFromNetwork(cancellationToken))
                {
                    IsRemovingNodes = true;
                    ThingManager.DetectChanges(this);
                    return true;
                }
            }
            return false;
        }

        [Operation]
        public async Task<bool> StopRemoveNodeFromNetworkAsync(CancellationToken cancellationToken)
        {
            if (_controller != null && !IsAddingNodes && IsRemovingNodes)
            {
                if (await _controller.StopRemoveNodeFromNetwork(cancellationToken))
                {
                    IsRemovingNodes = false;
                    ThingManager.DetectChanges(this);
                    return true;
                }
            }
            return false;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            if (_controller == null)
            {
                _controller = new ZWaveController(ActualSerialPort);
                _controller.Channel.MaxRetryCount = 15;

                _controller.ChannelClosed += (o, e) =>
                {
                    Logger.LogWarning("Z-Wave controller channel closed.");
                    Close();
                };

                _controller.Error += (o, e) =>
                {
                    if (e.Error is OperationCanceledException)
                    {
                        Close();
                        Logger.LogWarning("Z-Wave controller channel closed.");
                    }
                    else
                    {
                        Logger.LogWarning(e.Error, "Error in Z-Wave controller.");
                    }
                };

                _controller.NodesNetworkChanged += async (o, e) =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var refreshed = await RefreshAsync(CancellationToken.None);
                        if (refreshed)
                        {
                            break;
                        }

                        await Task.Delay(5000);
                    }
                };

                try
                {
                    _controller.Open();
                    IsConnected = true;

                    Version = await _controller.GetVersion(cancellationToken);
                }
                catch (Exception e)
                {
                    IsConnected = false;
                    _controller = null;

                    Logger.LogWarning(e, "Failed to open Z-Wave serial port.");
                }
            }

            ThingManager.DetectChanges(this);

            await RefreshAsync(cancellationToken);
            await RefreshMetadataAsync(cancellationToken);
        }

        [Operation]
        public async Task<bool> RefreshAsync(CancellationToken cancellationToken)
        {
            lock (this)
            {
                if (IsRefreshingNodes)
                {
                    return false;
                }
                IsRefreshingNodes = true;
            }

            try
            {
                if (_controller != null)
                {
                    try
                    {
                        Logger.LogDebug("Refreshing Z-Wave controller...");
                        var nodes = await _controller.DiscoverNodes(cancellationToken);

                        var things = new List<IThing>();
                        foreach (var node in nodes)
                        {
                            var thing = Things
                                .OfType<ZwaveDevice>()
                                .SingleOrDefault(d => d.NodeId == node.NodeID)
                                ?? new ZwaveDevice(node, this);

                            things.Add(thing);

                            RegisterEvents(node, thing);
                        }

                        Things = things.ToArray();
                    }
                    catch (Exception e)
                    {
                        Logger.LogWarning(e, "Failed to refresh Z-Wave controller nodes.");
                        Close();
                    }
                }

                ThingManager.DetectChanges(this);

                Logger.LogDebug("Refreshing Z-Wave devices info...");

                var tasks = new List<Task>();
                foreach (var thing in Things.OfType<ZwaveDevice>())
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await thing.TryRefreshInfoAsync(cancellationToken);
                    }, cancellationToken));
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to refresh Z-Wave controller.");
            }
            finally
            {
                IsRefreshingNodes = false;
            }
            return true;
        }

        private async Task RefreshMetadataAsync(CancellationToken cancellationToken)
        {
            lock (this)
            {
                if (IsRefreshingMetadata)
                {
                    return;
                }
                IsRefreshingMetadata = true;
            }

            try
            {
                ThingManager.DetectChanges(this);

                var tasks = new List<Task>();
                foreach (var thing in Things.OfType<ZwaveDevice>())
                {
                    if (thing.IsListening == true)
                    {
                        tasks.Add(Task.Run(async () =>
                        {
                            await thing.RefreshAsync(cancellationToken);
                        }, cancellationToken));
                    }
                }

                await Task.WhenAll(tasks);
                ThingManager.DetectChanges(this);
            }
            catch (Exception e)
            {
                Logger.LogWarning(e, "Failed to refresh Z-Wave metadata.");
            }
            finally
            {
                IsRefreshingMetadata = false;
            }
        }

        private void RegisterEvents(Node node, ZwaveDevice thing)
        {
            node.MessageReceived -= thing.OnMessageReceived;
            node.MessageReceived += thing.OnMessageReceived;

            node.UpdateReceived -= thing.OnUpdateReceived;
            node.UpdateReceived += thing.OnUpdateReceived;

            node.UnknownCommandReceived -= thing.OnUnknownCommandReceived;
            node.UnknownCommandReceived += thing.OnUnknownCommandReceived;

            var notification = node.GetCommandClass<Notification>();
            notification.Changed -= thing.OnNotification;
            notification.Changed += thing.OnNotification;

            var multiChannel = node.GetCommandClass<MultiChannel>();
#pragma warning disable CS0618 // Type or member is obsolete
            multiChannel.Changed -= thing.OnMultiChannel;
            multiChannel.Changed += thing.OnMultiChannel;
#pragma warning restore CS0618 // Type or member is obsolete

            var centralScene = node.GetCommandClass<CentralScene>();
            centralScene.Changed -= thing.OnCentralScene;
            centralScene.Changed += thing.OnCentralScene;

            var wakeUp = node.GetCommandClass<WakeUp>();
            wakeUp.Changed -= thing.OnWakeUp;
            wakeUp.Changed += thing.OnWakeUp;

            var battery = node.GetCommandClass<Battery>();
            battery.Changed -= thing.OnBattery;
            battery.Changed += thing.OnBattery;

            var sensorBinary = node.GetCommandClass<SensorBinary>();
            sensorBinary.Changed -= thing.OnSensorBinary;
            sensorBinary.Changed += thing.OnSensorBinary;

            var basic = node.GetCommandClass<Basic>();
            basic.Changed -= thing.OnBasic;
            basic.Changed += thing.OnBasic;

            var sensorAlarm = node.GetCommandClass<SensorAlarm>();
            sensorAlarm.Changed -= thing.OnSensorAlarm;
            sensorAlarm.Changed += thing.OnSensorAlarm;

            var sensorMultiLevel = node.GetCommandClass<SensorMultiLevel>();
            sensorMultiLevel.Changed -= thing.OnSensorMultiLevel;
            sensorMultiLevel.Changed += thing.OnSensorMultiLevel;

            var switchBinary = node.GetCommandClass<SwitchBinary>();
            switchBinary.Changed -= thing.OnSwitchBinary;
            switchBinary.Changed += thing.OnSwitchBinary;

            var switchMultiLevel = node.GetCommandClass<SwitchMultiLevel>();
            switchMultiLevel.Changed -= thing.OnSwitchMultiLevel;
            switchMultiLevel.Changed += thing.OnSwitchMultiLevel;

            var alarm = node.GetCommandClass<Alarm>();
            alarm.Changed -= thing.OnAlarm;
            alarm.Changed += thing.OnAlarm;

            var meter = node.GetCommandClass<Meter>();
            meter.Changed -= thing.OnMeter;
            meter.Changed += thing.OnMeter;
        }

        private void Close()
        {
            try
            {
                _controller?.Close();
            }
            catch
            {
            }

            _controller = null;

            //Things = Array.Empty<IThing>();
            IsConnected = false;

            ThingManager.DetectChanges(this);
        }

        public override void Dispose()
        {
            Close();
            base.Dispose();
        }
    }
}

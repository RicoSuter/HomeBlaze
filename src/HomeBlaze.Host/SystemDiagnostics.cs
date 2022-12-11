using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Services.Things;
using Microsoft.Extensions.Logging;
using MudBlazor;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.Things
{
#pragma warning disable CA1416

    public class SystemDiagnostics : PollingThing, ILastUpdatedProvider
    {
        private static PerformanceCounter? _cpuCounter;
        private static PerformanceCounter? _ramCounter;
        private static int _processorCount = 1;

        private readonly SystemThing _systemThing;
        private readonly IEventManager _eventManager;

        public override string? Id => "system.diagnostics." + _systemThing.InternalId;

        public override string Title => "System Diagnostics";

        public DateTimeOffset? LastUpdated { get; private set; }

        [State]
        public DateTimeOffset? StartupTime { get; private set; }

        [State]
        public string Runtime => RuntimeInformation.RuntimeIdentifier;

        [State("OperatingSystem")]
        public string OperatingSystem => RuntimeInformation.OSDescription;

        [State]
        public int ProcessorCount { get; private set; }

        [State(Unit = StateUnit.Kilobyte)]
        public decimal? MemoryUsage { get; private set; }

        [State(Unit = StateUnit.Percent)]
        public decimal? CpuUsage { get; private set; }

        [State]
        public int EventQueueSize => _eventManager.QueueSize;

        public SystemDiagnostics(SystemThing systemThing, IThingManager thingManager, IEventManager eventManager, ILogger logger)
            : base(thingManager, logger)
        {
            _systemThing = systemThing;
            _eventManager = eventManager;
        }

        [Operation]
        public async Task ExecuteCommandAsync(string command, string args, IDialogService dialogService)
        {
            try
            {
                var result = RunCommand(command, args);
                await dialogService.ShowMessageBox("Result", result);
            }
            catch (Exception e)
            {
                await dialogService.ShowMessageBox("Error", e.ToString());
            }
        }

        private string RunCommand(string command, string args)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Normal,
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (string.IsNullOrEmpty(error)) { return output; }
            else { return error; }
        }

        static SystemDiagnostics()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var instanceName = TryGetProcessInstanceName(process.Id);
                if (instanceName != null)
                {
                    _processorCount = Environment.ProcessorCount;

                    _cpuCounter = new PerformanceCounter("Process", "% Processor Time", instanceName);
                    _ramCounter = new PerformanceCounter("Process", "Working Set", instanceName);
                }
            }
            catch
            {
            }
        }

        public override Task PollAsync(CancellationToken cancellationToken)
        {
            if (StartupTime == null)
            {
                StartupTime = DateTimeOffset.Now;
            }

            try
            {
                ProcessorCount = _processorCount;
                MemoryUsage = (decimal?)(_ramCounter?.NextValue() / 1024.0);
                CpuUsage = (decimal?)(_cpuCounter?.NextValue() / _processorCount);
            }
            catch
            {
            }

            if (MemoryUsage == null)
            {
                try
                {
                    MemoryUsage = Process.GetCurrentProcess().WorkingSet64 / 1024.0m;
                }
                catch
                {
                }
            }

            LastUpdated = DateTimeOffset.Now;
            return Task.CompletedTask;
        }

        private static string? TryGetProcessInstanceName(int pid)
        {
            var category = new PerformanceCounterCategory("Process");
            var instances = category.GetInstanceNames();
            foreach (var instance in instances)
            {
                using (var counter = new PerformanceCounter("Process", "ID Process", instance, true))
                {
                    if (pid == counter.RawValue)
                    {
                        return instance;
                    }
                }
            }

            return null;
        }
    }
}
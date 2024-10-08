﻿using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Devices.Energy;
using HomeBlaze.Abstractions.Networking;
using HomeBlaze.Abstractions.Presentation;
using HomeBlaze.Abstractions.Sensors;
using HomeBlaze.MyStrom.Model;
using HomeBlaze.Services.Abstractions;
using Microsoft.Extensions.Logging;
using MudBlazor;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBlaze.MyStrom
{
    [DisplayName("myStrom Switch")]
    public class MyStromSwitch : PollingThing,
        ILastUpdatedProvider, IIconProvider,
        IConnectedThing, IPowerConsumptionSensor, ITemperatureSensor, IPowerRelay,
        INetworkAdapter
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public override string Title => string.IsNullOrEmpty(DisplayTitle) ?
            $"myStrom Switch ({Information?.MacAddress})" : DisplayTitle;

        public DateTimeOffset? LastUpdated { get; private set; }

        public string IconName => "fas fa-plug";

        public Color IconColor =>
            IsOn == true ? Color.Success :
            IsOn == false ? Color.Warning :
            Color.Default;

        [Configuration("title")]
        public string? DisplayTitle { get; set; }

        [Configuration]
        public string? IpAddress { get; set; }

        [Configuration]
        public decimal TemperatureOffset { get; set; } = 0;

        [Configuration]
        public bool AllowTurnOff { get; set; } = true;

        [Configuration]
        public int RefreshInterval { get; set; } = 15 * 1000;

        internal MyStromSwitchReport? ReportResponse { get; private set; }

        internal MyStromSwitchTemperature? TemperatureResponse { get; private set; }

        [ScanForState]
        internal MyStromSwitchInformation? Information { get; private set; }

        [State]
        public bool IsConnected { get; private set; }

        [State]
        public decimal? PowerConsumption => IsConnected ? Math.Round(ReportResponse?.Power + 1.0m ?? 1.0m, 1) : null;

        [State]
        public decimal? Temperature => IsConnected && TemperatureResponse != null ?
            Math.Round(TemperatureResponse.Compensated + (TemperatureResponse.Offset ?? 0) + TemperatureOffset, 2) :
            null;

        [State]
        public bool? IsOn => IsConnected && ReportResponse != null ? ReportResponse.Relay : null;

        protected override TimeSpan PollingInterval => TimeSpan.FromMilliseconds(RefreshInterval);

        public MyStromSwitch(
            IHttpClientFactory httpClientFactory,
            ILogger<MyStromSwitch> logger)
            : base(logger)
        {
            _httpClientFactory = httpClientFactory;
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                if (Information == null)
                {
                    var infoResponse = await httpClient.GetAsync($"http://{IpAddress}/info", cancellationToken);
                    var json = await infoResponse.Content.ReadAsStringAsync(cancellationToken);
                    if (json != "Not Found")
                    {
                        Information = JsonSerializer.Deserialize<MyStromSwitchInformation>(json);
                    }
                }

                if (Information is not null)
                {
                    await RefreshReportAsync(cancellationToken);
                }
            }
            catch
            {
                IsConnected = false;
                throw;
            }
        }

        public override void Reset()
        {
            Information = null;
            ReportResponse = null;
            TemperatureResponse = null;
            base.Reset();
        }

        [Operation]
        public async Task TurnOnAsync(CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            await httpClient.GetAsync($"http://{IpAddress}/relay?state=1", cancellationToken);
            await RefreshReportAsync(cancellationToken);
            DetectChanges(this);
        }

        [Operation]
        public async Task TurnOffAsync(CancellationToken cancellationToken)
        {
            if (AllowTurnOff)
            {
                using var httpClient = _httpClientFactory.CreateClient();
                await httpClient.GetAsync($"http://{IpAddress}/relay?state=0", cancellationToken);
                await RefreshReportAsync(cancellationToken);
                DetectChanges(this);
            }
        }

        private async Task RefreshReportAsync(CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient();

            var reportResponse = await httpClient.GetAsync($"http://{IpAddress}/report", cancellationToken);
            reportResponse.EnsureSuccessStatusCode();

            var reportJson = await reportResponse.Content.ReadAsStringAsync(cancellationToken);
            ReportResponse = JsonSerializer.Deserialize<MyStromSwitchReport>(reportJson);

            var temperatureResponse = await httpClient.GetAsync($"http://{IpAddress}/api/v1/temperature", cancellationToken);
            temperatureResponse.EnsureSuccessStatusCode();

            var temperatureJson = await temperatureResponse.Content.ReadAsStringAsync(cancellationToken);
            TemperatureResponse = JsonSerializer.Deserialize<MyStromSwitchTemperature>(temperatureJson);

            LastUpdated = DateTimeOffset.Now;

            IsConnected = true;
        }
    }
}

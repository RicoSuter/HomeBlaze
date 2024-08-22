using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Abstractions.Services;
using SpeedTestSharp.Client;
using SpeedTestSharp.Enums;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HomeBlaze.SpeedtestNet
{
    [DisplayName("Speedtest.net Internet Speed")]
    public class SpeedtestNet : IThing, ILastUpdatedProvider
    {
        private readonly IThingManager _thingManager;

        public string Title => "Internet speed (speedtest.net)";

        [Configuration(IsIdentifier = true)]
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        // State

        [State(Unit = StateUnit.MegabitsPerSecond)]
        public decimal? DownloadSpeed { get; set; }

        [State(Unit = StateUnit.MegabitsPerSecond)]
        public decimal? UploadSpeed { get; set; }

        [State]
        public TimeSpan? Latency { get; set; }

        [State]
        public bool IsLoading { get; private set; }

        public DateTimeOffset? LastUpdated { get; private set; }

        public SpeedtestNet(IThingManager thingManager)
        {
            _thingManager = thingManager;
        }

        [Operation]
        public async Task RefreshAsync()
        {
            try
            {
                if (IsLoading)
                {
                    return;
                }

                IsLoading = true;
                _thingManager.DetectChanges(this);

                var speedTestClient = new SpeedTestClient();
                var result = await speedTestClient.TestSpeedAsync(SpeedUnit.Mbps);
                DownloadSpeed = Math.Round((decimal)result.DownloadSpeed, 2);
                UploadSpeed = Math.Round((decimal)result.UploadSpeed, 2);
                Latency = TimeSpan.FromMilliseconds(result.Latency);
                LastUpdated = DateTimeOffset.Now;
            }
            finally
            {
                IsLoading = false;
            }
            _thingManager.DetectChanges(this);
        }
    }
}

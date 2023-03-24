using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Hosting;

namespace HomeBlaze.Services
{
    public class CombinedStateManager : BackgroundService, IDisposable, IStateManager
    {
        private readonly InfluxStateManager _influxStateManager;
        private readonly BlobStateManager _blobStateManager;

        public CombinedStateManager(InfluxStateManager influxStateManager, BlobStateManager blobStateManager)
        {
            _influxStateManager = influxStateManager;
            _blobStateManager = blobStateManager;
        }

        public Task<(DateTimeOffset, TState?)[]> ReadStateAsync<TState>(
            string thingId, string propertyName, 
            DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
        {
            return _influxStateManager.ReadStateAsync<TState>(
                thingId, propertyName, from, to, cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _blobStateManager.StartAsync(stoppingToken);
        }

        public override void Dispose()
        {
            base.Dispose();

            _influxStateManager.Dispose();
            _blobStateManager.Dispose();
        }
    }
}
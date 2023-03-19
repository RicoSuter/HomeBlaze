using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HomeBlaze.Services.Abstractions
{
    public abstract class PollingThing : BackgroundService, IThing
    {
        private readonly ILogger _logger;

        public abstract string? Id { get; }

        public abstract string? Title { get; }

        protected virtual TimeSpan PollingInterval => TimeSpan.FromSeconds(30);

        protected virtual TimeSpan FailureInterval => TimeSpan.FromSeconds(60);

        public IThingManager ThingManager { get; private set; }

        public PollingThing(
            IThingManager thingManager,
            ILogger logger)
        {
            ThingManager = thingManager;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PollAsync(stoppingToken);
                    ThingManager.DetectChanges(this);

                    await Task.Delay(PollingInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception e)
                {
                    ThingManager.DetectChanges(this);

                    _logger.LogWarning(e, "Polling of thing failed.");
                    await Task.Delay(FailureInterval, stoppingToken);
                }
            }
        }

        public abstract Task PollAsync(CancellationToken cancellationToken);
    }
}
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Messages;
using Microsoft.Extensions.Hosting;

namespace HomeBlaze.Services
{
    public class TimeMessagePublisher : BackgroundService
    {
        private readonly IEventManager _eventManager;
        private long _lastSeconds;

        public TimeMessagePublisher(IEventManager eventManager)
        {
            _lastSeconds = DateTimeOffset.Now.ToUnixTimeSeconds();
            _eventManager = eventManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(100, stoppingToken);

                var now = DateTimeOffset.Now;
                var nowSeconds = now.ToUnixTimeSeconds();

                for (var timeToPublish = _lastSeconds + 1; timeToPublish <= nowSeconds; timeToPublish++)
                {
                    _eventManager.Publish(new TimerEvent
                    {
                        Source = this,
                        DateTime = DateTimeOffset
                            .FromUnixTimeSeconds(timeToPublish)
                            .ToOffset(now.Offset)
                    });
                }

                _lastSeconds = nowSeconds;
            }
        }
    }
}
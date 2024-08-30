using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Attributes;
using HomeBlaze.Messages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reactive.Subjects;

namespace HomeBlaze.Services.Abstractions
{
    public abstract class PollingThing : BackgroundService, IThing, IObservable<DetectChangesEvent>
    {
        private readonly ILogger _logger;

        private Subject<DetectChangesEvent>? _detectChanges = new();

        [Configuration(IsIdentifier = true)]
        public virtual string Id { get; set; } = Guid.NewGuid().ToString();

        public abstract string? Title { get; }

        protected virtual TimeSpan PollingInterval => TimeSpan.FromSeconds(30);

        protected virtual TimeSpan FailureInterval => TimeSpan.FromSeconds(60);

        public PollingThing(ILogger logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PollAsync(stoppingToken);
                    DetectChanges(this);

                    // use for-loop to allow polling interval changes
                    for (int i = 0; i < PollingInterval.TotalSeconds; i++)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    }
                }
                catch (TaskCanceledException)
                {
                }
                catch (Exception e)
                {
                    DetectChanges(this);
                    _logger.LogWarning(e, "Polling of thing failed.");

                    // use for-loop to allow polling interval changes
                    for (int i = 0; i < FailureInterval.TotalSeconds; i++)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    }
                }
            }
        }
        
        public void DetectChanges(IThing thing)
        {
            _detectChanges?.OnNext(new DetectChangesEvent(thing));
        }

        public abstract Task PollAsync(CancellationToken cancellationToken);

        public IDisposable Subscribe(IObserver<DetectChangesEvent> observer)
        {
            return _detectChanges?.Subscribe(observer) ?? throw new ObjectDisposedException(Id);
        }

        public override void Dispose()
        {
            base.Dispose();
            _detectChanges?.Dispose();
            _detectChanges = null;
        }
    }
}
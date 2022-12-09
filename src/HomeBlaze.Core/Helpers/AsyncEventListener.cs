using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace HomeBlaze.Helpers
{
    /// <summary>
    /// An event listener which receives all events and processes them on another thread (non-blocking for other event consumers).
    /// </summary>
    public abstract class AsyncEventListener : BackgroundService, IDisposable
    {
        private readonly BlockingCollection<IEvent> _queue = new BlockingCollection<IEvent>();
        private readonly IDisposable _subscription;

        public AsyncEventListener(IEventManager eventManager)
        {
            _subscription = eventManager.Subscribe(@event => _queue.Add(@event));
        }

        protected abstract Task HandleMessageAsync(IEvent message, CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queue.TryTake(out var @event, 1000, stoppingToken))
                {
                    await HandleMessageAsync(@event, stoppingToken);
                }
            }
        }

        public override void Dispose()
        {
            _subscription?.Dispose();
            base.Dispose();
        }
    }
}
using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks.Dataflow;

namespace HomeBlaze.Services.Abstractions
{
    /// <summary>
    /// An event listener which receives all events and processes them on another thread (non-blocking for other event consumers).
    /// </summary>
    public abstract class AsyncEventListener : BackgroundService, IDisposable
    {
        private readonly BufferBlock<IEvent> _queue = new BufferBlock<IEvent>();
        private readonly IDisposable _subscription;

        public AsyncEventListener(IEventManager eventManager)
        {
            _subscription = eventManager.Subscribe(@event => _queue.Post(@event));
        }

        protected abstract Task HandleMessageAsync(IEvent message, CancellationToken cancellationToken);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield(); // required: do not block ASP startup
            while (!stoppingToken.IsCancellationRequested)
            {
                var @event = await _queue.ReceiveAsync(TimeSpan.FromMilliseconds(-1), stoppingToken);
                if (@event is not null)
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
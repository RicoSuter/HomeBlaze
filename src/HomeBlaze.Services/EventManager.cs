using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Hosting;
using System.Reactive.Subjects;
using System.Threading.Tasks.Dataflow;

namespace HomeBlaze.Services
{
    public class EventManager : BackgroundService, IEventManager
    {
        private readonly BufferBlock<IEvent> _queue = new BufferBlock<IEvent>();
        private readonly Subject<IEvent> _subject = new Subject<IEvent>();

        public int QueueSize => _queue.Count;

        public void OnCompleted()
        {
            // TODO: Write log
        }

        public void OnError(Exception error)
        {
            // TODO: Write log
        }

        public void OnNext(IEvent value)
        {
            _queue.Post(value);
        }

        public void Publish(IEvent @event)
        {
            _queue.Post(@event);
        }

        public IDisposable Subscribe(IObserver<IEvent> observer)
        {
            return _subject.Subscribe(observer);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var @event = await _queue.ReceiveAsync(TimeSpan.FromMilliseconds(-1), stoppingToken);
                if (@event is not null)
                {
                    _subject.OnNext(@event);
                }
            }
        }
    }
}
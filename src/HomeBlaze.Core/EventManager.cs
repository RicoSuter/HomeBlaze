using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Abstractions.Services;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace HomeBlaze
{
    public class EventManager : BackgroundService, IEventManager
    {
        private readonly BlockingCollection<IEvent> _queue = new BlockingCollection<IEvent>();
        private readonly Subject<IEvent> _subject = new Subject<IEvent>();

        public int QueueSize => _queue.Count;

        public void Publish(IEvent @event)
        {
            _queue.Add(@event);
        }

        public IDisposable Subscribe(IObserver<IEvent> observer)
        {
            return _subject.Subscribe(observer);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_queue.TryTake(out var @event, 1000, stoppingToken))
                    {
                        _subject.OnNext(@event);
                    }
                }
            }, stoppingToken);
        }
    }
}
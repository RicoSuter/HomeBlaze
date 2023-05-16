using HomeBlaze.Abstractions.Messages;

namespace HomeBlaze.Abstractions.Services
{
    public interface IEventManager : IObservable<IEvent>
    {
        int QueueSize { get; }

        void Publish(IEvent @event);
    }
}
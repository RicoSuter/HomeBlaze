using Namotion.Devices.Abstractions.Messages;

namespace HomeBlaze.Abstractions.Services
{
    public interface IEventManager : 
        IObservable<IEvent>, 
        IObserver<IEvent>
    {
        int QueueSize { get; }

        void Publish(IEvent @event);
    }
}
namespace HomeBlaze.Abstractions.Messages
{
    public interface IThingEvent : IEvent
    {
        string ThingId { get; }
    }
}
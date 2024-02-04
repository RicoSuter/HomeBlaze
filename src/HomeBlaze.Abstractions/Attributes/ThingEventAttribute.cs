namespace HomeBlaze.Abstractions.Attributes
{
    /// <summary>
    /// Specifies that the given thing emits an event.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ThingEventAttribute : Attribute
    {
        public ThingEventAttribute(Type eventType)
        {
            EventType = eventType;
        }

        public Type EventType { get; }
    }
}
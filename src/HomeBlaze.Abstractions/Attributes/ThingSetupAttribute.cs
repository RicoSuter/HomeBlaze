namespace HomeBlaze.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ThingSetupAttribute : Attribute
    {
        public ThingSetupAttribute(Type componentType)
        {
            ComponentType = componentType;
        }

        public Type ComponentType { get; }

        public bool CanEdit { get; set; }
    }
}
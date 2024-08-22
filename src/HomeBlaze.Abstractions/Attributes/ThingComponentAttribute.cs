namespace HomeBlaze.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ThingComponentAttribute : Attribute
    {
        public ThingComponentAttribute(Type thingType)
        {
            ThingType = thingType;
        }

        /// <summary>
        /// Gets or sets the type of the setup component.
        /// </summary>
        public Type ThingType { get; internal set; }

        /// <summary>
        /// Gets or sets the type of the setup component.
        /// </summary>
        public Type? ComponentType { get; set; }
    }
}
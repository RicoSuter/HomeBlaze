namespace HomeBlaze.Abstractions.Attributes
{
    /// <summary>
    /// Will inject the parent thing into the given property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ParentThingAttribute : Attribute
    {
    }
}
namespace HomeBlaze.Abstractions.Attributes
{
    /// <summary>
    /// Will inject the parent thing into the given property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ParentThingAttribute : Attribute
    {
    }
}
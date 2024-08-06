namespace HomeBlaze.Abstractions.Attributes
{
    /// <summary>
    /// Marks a property to be scanned for additional state attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class ScanForStateAttribute : Attribute
    {
    }
}
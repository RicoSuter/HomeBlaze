namespace HomeBlaze.Abstractions.Attributes
{
    /// <summary>
    /// Marks a property to be scanned for additional state attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ScanForStateAttribute : Attribute
    {
    }
}
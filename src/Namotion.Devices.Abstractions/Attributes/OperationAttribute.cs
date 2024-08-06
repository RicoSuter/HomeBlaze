namespace HomeBlaze.Abstractions.Attributes
{
    /// <summary>
    /// Marks a method as operation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class OperationAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the title of the operation which overrides the method name.
        /// </summary>
        public string? Title { get; set; }
    }
}
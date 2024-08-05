namespace HomeBlaze.Abstractions.Attributes
{
    /// <summary>
    /// Provides additional metadata to the parameter of an operation parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = true)]
    public class OperationParameterAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the unit of the parameter value.
        /// </summary>
        public StateUnit Unit { get; set; }
    }
}
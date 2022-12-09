namespace HomeBlaze.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigurationAttribute : Attribute
    {
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this configuration 
        /// stores a secret value so it is e.g. not shown in the UI.
        /// </summary>
        public bool IsSecret { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the storage for the thing's ID property.
        /// Defining this is required for cloning things so that this property can be ignored.
        /// The property "Id" is implicitly treated as identifier property.
        /// </summary>
        public bool IsIdentifier { get; set; }

        public ConfigurationAttribute(string? name = null)
        {
            Name = name;
        }
    }
}
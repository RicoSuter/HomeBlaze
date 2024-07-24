using NJsonSchema.Converters;

namespace HomeBlaze.Abstractions
{
    [JsonInheritanceConverter(typeof(IThing))]
    public interface IThing
    {
        /// <summary>
        /// Gets the ID of the thing. 
        /// The ID must be constant (i.e. the property must always return the same value).
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the display title of the thing.
        /// </summary>
        string? Title { get; }
    }
}
using HomeBlaze.Abstractions.Json;

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
        
        /// <summary>
        /// Resets the thing back to the after loaded state (e.g. to reload with new configuration).
        /// </summary>
        public void Reset()
        {
        }
    }
}
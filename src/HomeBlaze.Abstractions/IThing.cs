using HomeBlaze.Abstractions.Json;

namespace HomeBlaze.Abstractions
{
    [JsonInheritanceConverter(typeof(IThing))]
    public interface IThing
    {
        /// <summary>
        /// Gets the ID of the thing. 
        /// The ID should be stable even after recreation of the thing (use e.g. the device's serial number). 
        /// Should be null while initialization until the final stable ID is available.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the display title of the thing.
        /// </summary>
        string? Title { get; }
    }
}
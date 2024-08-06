using System.Reflection;

namespace HomeBlaze.Abstractions.Attributes
{
    /// <summary>
    /// Defines a property as part of the thing's state.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class StateAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the state name (if not null, overrides the property name).
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// Gets or sets the unit of the state property.
        /// </summary>
        public StateUnit Unit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value being tracked is a total value, representing the accumulation of multiple occurrences or events over time.
        /// </summary>
        public bool IsCumulative { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this state is a signal (i.e. not an imprecise sensor value).
        /// </summary>
        public bool IsSignal { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the value is estimated (usually based on other values).
        /// </summary>
        public bool IsEstimated { get; set; }

        /// <summary>
        /// Gets or sets the order of the property.
        /// </summary>
        public int Order { get; set; } // TODO: Also apply this in LoadState (not just the UI).

        public StateAttribute(string? name = null)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the actual property name based on the property info or the name property.
        /// </summary>
        /// <param name="thing">The thing.</param>
        /// <param name="property">The property info.</param>
        /// <returns>The property name.</returns>
        public virtual string GetPropertyName(object thing, PropertyInfo property)
        {
            return Name ?? property.Name;
        }

        /// <summary>
        /// Gets the display text of the given value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The display text.</returns>
        public virtual string GetDisplayText(object? value)
        {
            if (value == null)
            {
                return "";
            }

            if (value is TimeSpan timeSpan)
            {
                return timeSpan.TotalSeconds < 5 ? timeSpan.TotalMilliseconds + " ms" :
                                                   timeSpan.TotalHours + " h";
            }

            return Unit switch
            {
                StateUnit.Percent => (int)(((decimal?)value) * 100m) + "%",
                StateUnit.DegreeCelsius => value + " °C",
                StateUnit.Watt => value + " W",
                StateUnit.Hertz => value + " hz",
                StateUnit.KiloWatt => value + " kW",
                StateUnit.WattHour => value + " Wh",
                StateUnit.Lumen => value + " lm",
                StateUnit.Meter => value + " m",
                StateUnit.Millimeter => value + " mm",
                StateUnit.MillimeterPerHour => value + " mm/h",
                StateUnit.Kilobyte => value + " kb",
                StateUnit.KilobytePerSecond => value + " kb/s",
                StateUnit.MegabitsPerSecond => value + " MBit/s",
                StateUnit.LiterPerHour => value + " l/h",
                StateUnit.Default => value?.ToString() ?? "",
                _ => $"{value} {Unit}",
            };
        }
    }
}
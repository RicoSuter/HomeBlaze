namespace HomeBlaze.Abstractions.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class StateAttribute : Attribute
    {
        public string? Name { get; }

        public StateUnit Unit { get; set; }

        public bool IsAggregation { get; set; }

        public bool IsSignal { get; set; }

        public StateAttribute(string? name = null)
        {
            Name = name;
        }

        public virtual string GetPropertyName(IThing thing, System.Reflection.PropertyInfo property)
        {
            return Name ?? property.Name;
        }

        public virtual string GetDisplayText(object? value)
        {
            if (value == null)
            {
                return "";
            }

            return Unit switch
            {
                StateUnit.Percent => (int)(((decimal?)value) * 100m) + "%",
                StateUnit.DegreeCelsius => value + " °C",
                StateUnit.WattPerHour => value + " Wh/h",
                StateUnit.Lumen => value + " lm",
                StateUnit.Kilobyte => value + " kb",
                StateUnit.Default => value?.ToString() ?? "",
                _ => $"{value} {Unit}",
            };
        }
    }
}
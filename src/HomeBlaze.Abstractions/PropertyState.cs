using HomeBlaze.Abstractions.Attributes;
using Namotion.Reflection;

namespace HomeBlaze.Abstractions
{
    public struct PropertyState
    {
        public string Name { get; init; }

        public object? Value { get; init; }

        public object? PreviousValue { get; set; }

        public IThing SourceThing { get; init; }

        public DateTimeOffset? LastUpdated { get; set; }

        public DateTimeOffset? LastChanged { get; set; }

        // Metadata

        public bool HasThingChildren =>
            Property?.PropertyType.Type.IsAssignableTo(typeof(IThing)) == true ||
            Property?.PropertyType.Type.IsAssignableTo(typeof(IEnumerable<IThing>)) == true;

        public IEnumerable<IThing> Children => 
            Value is IThing thingChild ? new IThing[] { thingChild } :
            Value is IEnumerable<IThing> thingsChildren ? thingsChildren :
            Array.Empty<global::HomeBlaze.Abstractions.IThing>();

        public StateAttribute Attribute { get; init; }

        public ContextualPropertyInfo? Property { get; set; }

        public string GetDisplayText()
        {
            return Attribute?.GetDisplayText(Value) ?? Value?.ToString() ?? string.Empty;
        }
        public string GetPreviousDisplayText()
        {
            return Attribute?.GetDisplayText(PreviousValue) ?? PreviousValue?.ToString() ?? string.Empty;
        }
    }
}
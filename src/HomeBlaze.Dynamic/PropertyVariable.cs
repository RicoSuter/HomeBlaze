using HomeBlaze.Abstractions.Messages;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Components.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace HomeBlaze.Dynamic
{
    public enum WindowAggregation
    {
        Average,
        Minimum,
        Maximum
    }

    public class PropertyVariable
    {
        public string? Name { get; set; }

        public string? ThingId { get; set; }

        public string? Property { get; set; }

        public TimeSpan? WindowDuration { get; set; }

        public WindowAggregation? WindowAggregation { get; set; }

        [JsonIgnore]
        public string ActualName => Name ?? Property ?? "unnamed";

        private Dictionary<DateTimeOffset, object?> _values = new();

        public object? TryGetValue(IThingManager thingManager)
        {
            if (WindowDuration != null)
            {
                lock (_values)
                {
                    var segments = _values
                        .OrderBy(v => v.Key)
                        .GetSegments()
                        .Where(s => s.First.Value != null)
                        .ToArray();

                    switch (WindowAggregation)
                    {
                        case Dynamic.WindowAggregation.Minimum:
                            return segments.Min(s => (decimal)s.First.Value! * (int)(s.Second.Key - s.First.Key).TotalMilliseconds);

                        case Dynamic.WindowAggregation.Maximum:
                            return segments.Max(s => (decimal)s.First.Value! * (int)(s.Second.Key - s.First.Key).TotalMilliseconds);

                        case Dynamic.WindowAggregation.Average:
                            var a = segments.Sum(s => (decimal)s.First.Value! * (int)(s.Second.Key - s.First.Key).TotalMilliseconds);
                            var b = segments.Sum(s => (int)(s.Second.Key - s.First.Key).TotalMilliseconds);
                            return b > 0 ? Math.Round(a / b, 3) : null;

                        default: return null;
                    }
                }
            }
            else if (ThingId != null && Property != null)
            {
                var state = thingManager.TryGetPropertyState(ThingId, Property, true);
                return state?.Value;
            }
            else
            {
                return null;
            }
        }

        public void Apply(ThingStateChangedEvent stateChangedEvent)
        {
            if (WindowDuration != null)
            {
                lock (_values)
                {
                    _values[stateChangedEvent.ChangeDate] = stateChangedEvent.NewValue;

                    foreach (var pair in _values.Where(v => v.Key < stateChangedEvent.ChangeDate - WindowDuration))
                    {
                        _values.Remove(pair.Key);
                    }
                }
            }
        }
    }
}

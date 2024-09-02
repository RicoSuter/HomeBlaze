using HomeBlaze.Abstractions;
using HomeBlaze.Abstractions.Services;
using HomeBlaze.Components.Extensions;
using HomeBlaze.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace HomeBlaze.Components.Editors
{
    public class PropertyVariable
    {
        public string? Name { get; set; }

        public string? ThingId { get; set; }

        public string? PropertyName { get; set; }

        public AggregationType WindowAggregation { get; set; } = AggregationType.None;

        public TimeSpan? WindowDuration { get; set; }

        [JsonIgnore]
        public string ActualName =>
            !string.IsNullOrEmpty(Name) ? Name :
            !string.IsNullOrEmpty(PropertyName) ? PropertyName :
            "unnamed";

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
                        case AggregationType.Average:
                            var a = segments.Sum(s => (decimal)s.First.Value! * (int)(s.Second.Key - s.First.Key).TotalMilliseconds);
                            var b = segments.Sum(s => (int)(s.Second.Key - s.First.Key).TotalMilliseconds);
                            return b > 0 ? Math.Round(a / b, 3) : null;

                        case AggregationType.Minimum:
                            return segments.Min(s => (decimal)s.First.Value! * (int)(s.Second.Key - s.First.Key).TotalMilliseconds);

                        case AggregationType.Maximum:
                            return segments.Max(s => (decimal)s.First.Value! * (int)(s.Second.Key - s.First.Key).TotalMilliseconds);

                        case AggregationType.Sum:
                            return segments.Sum(s => (decimal)s.First.Value! * (int)(s.Second.Key - s.First.Key).TotalMilliseconds);

                        default: 
                            return null;
                    }
                }
            }
            else if (ThingId != null && PropertyName != null)
            {
                var state = thingManager.TryGetPropertyState(ThingId, PropertyName, true);
                return state?.Value;
            }
            else
            {
                return null;
            }
        }

        public void Apply(ThingStateChangedEvent stateChangedEvent)
        {
            if (WindowAggregation != AggregationType.None && 
                WindowDuration != null &&
                ThingId == (stateChangedEvent.Source as IThing)?.Id &&
                PropertyName == stateChangedEvent.PropertyName)
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

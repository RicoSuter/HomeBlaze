using HomeBlaze.Abstractions;
using HomeBlaze.Components.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeBlaze.Components.Graphs
{
    internal static class ChartSeriesUtilities
    {
        public static ChartItem[] CleanUp(PropertyState? state, ChartItem[] values, int numberOfHours)
        {
            try
            {
                var startDate = DateTimeOffset.Now.AddHours(numberOfHours * -1);
                var endDate = DateTimeOffset.Now;
                var duration = endDate - startDate;

                if (state?.Property?.PropertyType.Type == typeof(decimal) ||
                    state?.Property?.PropertyType.Type == typeof(double) ||
                    state?.Property?.PropertyType.Type == typeof(float) ||
                    state?.Property?.PropertyType.Type == typeof(long) ||
                    state?.Property?.PropertyType.Type == typeof(int))
                {
                    var chartItems = new List<ChartItem>();

                    if (values.Length > 720)
                    {
                        var numberOfValues = numberOfHours < 24 * 2 ? 24 * 2 : numberOfHours;
                        var valueDuration = duration / numberOfValues;

                        for (int i = 0; i < numberOfValues; i++)
                        {
                            var start = startDate + valueDuration * i;
                            var end = startDate + valueDuration * (i + 1);

                            var filteredValues = values
                                .Where(v => v.DateTime >= start && v.DateTime < end)
                                .ToArray();

                            chartItems.Add(new ChartItem
                            {
                                Index = i,
                                DateTime = start + valueDuration / 2,
                                RawValue =
                                    filteredValues.Any() ?
                                    filteredValues
                                        .Select(v => v.Value)
                                        .Average() : null
                            });
                        }
                    }
                    else
                    {
                        chartItems = values.ToList();
                    }

                    return chartItems
                        .Where(v => v.RawValue != null)
                        .ToArray();
                }
                else if (state?.Property?.PropertyType.Type == typeof(bool))
                {
                    return values
                        .GetSegments()
                        .SelectMany(x =>
                        {
                            if (x.First.Value == 1 && x.Second.Value == 0)
                            {
                                return new[] { new ChartItem { DateTime = x.Second.DateTime.AddMilliseconds(-1), RawValue = true }, x.Second };
                            }
                            else if (x.First.Value == 0 && x.Second.Value == 1)
                            {
                                return new[] { new ChartItem { DateTime = x.Second.DateTime.AddMilliseconds(-1), RawValue = false }, x.Second };
                            }
                            else
                            {
                                return new[] { x.Second };
                            }
                        })
                        .ToArray();
                }
                else
                {
                    return Array.Empty<ChartItem>();
                }
            }
            catch
            {
                return Array.Empty<ChartItem>();
            }
        }
    }
}

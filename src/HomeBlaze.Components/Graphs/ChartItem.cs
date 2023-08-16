using System;

namespace HomeBlaze.Components.Graphs
{
    internal class ChartItem
    {
        public int Index { get; set; }

        public DateTimeOffset DateTime { get; set; }

        public DateTime Date => DateTime.DateTime;

        public object? RawValue { get; set; }

        public double Value => double
            .TryParse(RawValue?.ToString() ?? "", out var number) ?
                Math.Round(number, 2) :
                0;
    }
}
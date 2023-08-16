using System;

namespace HomeBlaze.Components.Graphs
{
    internal class ChartItem
    {
        public int Index { get; set; }

        public DateTimeOffset DateTime { get; set; }

        public DateTime Date => DateTime.DateTime;

        public object? RawValue { get; set; }

        public double Value
        {
            get
            {
                var valueString = RawValue?.ToString();
                if (valueString == "True")
                {
                    return 1;
                }
                else if (valueString == "False")
                {
                    return 0;
                }
                else if (double.TryParse(RawValue?.ToString() ?? "", out var number))
                {
                    return Math.Round(number, 2);
                }

                return 0;
            }
        }
    }
}
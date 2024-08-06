using MudBlazor;
using System;

namespace HomeBlaze.Components
{
    public static class MudExtensions
    {
        public static Color ToMudColor(this string color)
        {
            return Enum.TryParse<Color>(color, out var result) ? result : Color.Default;
        }
    }
}
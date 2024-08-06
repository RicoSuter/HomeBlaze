namespace HomeBlaze.Services
{
    public static class ColorUtilities
    {
        private static Random _random = new();

        public static string GenerateVibrantColor()
        {
            // Generate a random hue between 0 and 360
            double hue = _random.NextDouble() * 360;

            // Set saturation to a value between 70% and 100% to ensure vibrancy
            double saturation = 0.7 + (_random.NextDouble() * 0.3);

            // Use a lightness between 40% and 60% for good contrast on both light and dark themes
            double lightness = 0.4 + (_random.NextDouble() * 0.2);

            return ColorFromHSL(hue, saturation, lightness);
        }

        private static string ColorFromHSL(double h, double s, double l)
        {
            h /= 360.0;

            double r, g, b;

            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                r = HueToRgb(p, q, h + 1.0 / 3);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 1.0 / 3);
            }

            return $"#{(int)(r * 255):X2}{(int)(g * 255):X2}{(int)(b * 255):X2}";
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 0.5) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }
    }
}

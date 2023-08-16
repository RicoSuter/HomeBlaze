namespace HomeBlaze.Services
{
    public static class ColorUtilities
    {
        public static string GenerateVibrantColor()
        {
            Random random = new Random();

            // Generate a random hue between 0 and 360
            double hue = random.NextDouble() * 360;

            // Set saturation to a value between 50% and 100%
            double saturation = 0.5 + (random.NextDouble() * 0.5);

            // For simplicity, use a random lightness between 20% and 80% 
            // (avoiding too dark or too light colors)
            double lightness = 0.2 + (random.NextDouble() * 0.6);

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

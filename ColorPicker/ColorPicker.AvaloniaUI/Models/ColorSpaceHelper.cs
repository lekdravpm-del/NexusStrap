using System;

namespace ColorPicker.Models
{
    public static class ColorSpaceHelper
    {
        /// <summary>
        ///     Converts RGB to HSV, returns -1 for undefined channels
        /// </summary>
        /// <param name="r">Red channel</param>
        /// <param name="g">Green channel</param>
        /// <param name="b">Blue channel</param>
        /// <returns>Values in order: Hue (0-360 or -1), Saturation (0-1 or -1), Value (0-1)</returns>
        public static Tuple<double, double, double> RgbToHsv(double r, double g, double b)
        {
            double min, max, delta;
            double h, s, v;

            min = Math.Min(r, Math.Min(g, b));
            max = Math.Max(r, Math.Max(g, b));
            v = max;
            delta = max - min;
            if (max != 0)
            {
                s = delta / max;
            }
            else
            {
                //pure black
                s = -1;
                h = -1;
                return new Tuple<double, double, double>(h, s, v);
            }

            if (r == max)
                h = (g - b) / delta; // between yellow & magenta
            else if (g == max)
                h = 2 + (b - r) / delta; // between cyan & yellow
            else
                h = 4 + (r - g) / delta; // between magenta & cyan
            h *= 60;
            if (h < 0)
                h += 360;
            if (double.IsNaN(h)) //delta == 0, case of pure gray
                h = -1;

            return new Tuple<double, double, double>(h, s, v);
        }

        /// <summary>
        ///     Converts RGB to Grayscale Tuple
        /// </summary>
        /// <remarks>This method uses the luminosity method to calculate the grayscale</remarks>
        /// <param name="r">Red channel</param>
        /// <param name="g">Green channel</param>
        /// <param name="b">Blue channel</param>
        /// <returns>Values (0-1) in order: R, G, B (all same)</returns>
        public static Tuple<double, double, double> RgbToGrayTuple(double r, double g, double b)
        {
            // Using the luminosity method
            var gray = RgbToGray(r, g, b);
            return new Tuple<double, double, double>(gray, gray, gray);
        }

        /// <summary>
        ///     Converts RGB to Grayscale
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns>Grayscale value (0-1)</returns>
        public static double RgbToGray(double r, double g, double b)
        {
            // Using the luminosity method
            return 0.21 * r + 0.72 * g + 0.07 * b;
        }

        /// <summary>
        ///     Converts HSV to RGB
        /// </summary>
        /// <param name="h">Hue, 0-360</param>
        /// <param name="s">Saturation, 0-1</param>
        /// <param name="v">Value, 0-1</param>
        /// <returns>Values (0-1) in order: R, G, B</returns>
        public static Tuple<double, double, double> HsvToRgb(double h, double s, double v)
        {
            if (s == 0)
                // achromatic (grey)
                return new Tuple<double, double, double>(v, v, v);
            if (h >= 360.0)
                h = 0;
            h /= 60;
            var i = (int)h;
            var f = h - i;
            var p = v * (1 - s);
            var q = v * (1 - s * f);
            var t = v * (1 - s * (1 - f));

            switch (i)
            {
                case 0: return new Tuple<double, double, double>(v, t, p);
                case 1: return new Tuple<double, double, double>(q, v, p);
                case 2: return new Tuple<double, double, double>(p, v, t);
                case 3: return new Tuple<double, double, double>(p, q, v);
                case 4: return new Tuple<double, double, double>(t, p, v);
                default: return new Tuple<double, double, double>(v, p, q);
            }

            ;
        }

        /// <summary>
        ///     Converts HSV to Grayscale
        /// </summary>
        /// <param name="h">Hue, 0-360</param>
        /// <param name="s">Saturation, 0-1</param>
        /// <param name="v">Value, 0-1</param>
        /// <returns>Values (0-1) in order: R, G, B (all same)</returns>
        public static Tuple<double, double, double> HsvToGray(double h, double s, double v)
        {
            //Converting HSV to RGB first to not only use the value component
            var rgb = HsvToRgb(h, s, v);
            // Using the luminosity method
            var grey = RgbToGray(rgb.Item1, rgb.Item2, rgb.Item3);

            return new Tuple<double, double, double>(grey, grey, grey);
        }
    }
}
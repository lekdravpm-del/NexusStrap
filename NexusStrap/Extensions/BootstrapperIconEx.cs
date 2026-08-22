using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using Windows.Win32;

namespace NexusStrap.Extensions
{
    static class BootstrapperIconEx
    {
        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);
        public static IReadOnlyCollection<BootstrapperIcon> Selections => new BootstrapperIcon[]
        {
            //BootstrapperIcon.IconFishstrap,
            BootstrapperIcon.IconNexus,
            BootstrapperIcon.Icon8Bit,
            BootstrapperIcon.Icon2025,
            BootstrapperIcon.Icon2022,
            BootstrapperIcon.Icon2019,
            BootstrapperIcon.Icon2017,
            BootstrapperIcon.IconLate2015,
            BootstrapperIcon.IconEarly2015,
            BootstrapperIcon.Icon2011,
            BootstrapperIcon.Icon2008,
            BootstrapperIcon.IconClassic,
            BootstrapperIcon.IconCustom
        };

        // small note on handling icon sizes
        // i'm using multisize icon packs here with sizes 16, 24, 32, 48, 64 and 128
        // use this for generating multisize packs: https://www.aconvert.com/icon/

        public static Icon GetIcon(this BootstrapperIcon icon)
        {
            const string LOG_IDENT = "BootstrapperIconEx::GetIcon";

            // load the custom icon file
            if (icon == BootstrapperIcon.IconCustom)
            {
                Icon? customIcon = null;
                string location = App.Settings.Prop.BootstrapperIconCustomLocation;

                if (String.IsNullOrEmpty(location))
                {
                    App.Logger.WriteLine(LOG_IDENT, "Warning: custom icon is not set.");
                }
                else
                {
                    try
                    {
                        customIcon = new Icon(location);
                    }
                    catch (Exception ex)
                    {
                        App.Logger.WriteLine(LOG_IDENT, $"Failed to load custom icon!");
                        App.Logger.WriteException(LOG_IDENT, ex);
                    }
                }

                return customIcon ?? Properties.Resources.IconNexus;
            }

            return icon switch
            {
                // BootstrapperIcon.IconFishstrap => Properties.Resources.IconFishstrap,
                BootstrapperIcon.IconNexus => Properties.Resources.IconNexus,
                BootstrapperIcon.Icon8Bit => Pixelate(Properties.Resources.IconNexus),
                BootstrapperIcon.Icon2008 => Properties.Resources.Icon2008,
                BootstrapperIcon.Icon2011 => Properties.Resources.Icon2011,
                BootstrapperIcon.IconEarly2015 => Properties.Resources.IconEarly2015,
                BootstrapperIcon.IconLate2015 => Properties.Resources.IconLate2015,
                BootstrapperIcon.Icon2017 => Properties.Resources.Icon2017,
                BootstrapperIcon.Icon2019 => Properties.Resources.Icon2019,
                BootstrapperIcon.Icon2022 => Properties.Resources.Icon2022,
                BootstrapperIcon.Icon2025 => Properties.Resources.Icon2025,
                BootstrapperIcon.IconClassic => Properties.Resources.IconClassic,
                _ => Properties.Resources.IconNexus
            };
        }

        // turns an icon into a chunky 8-bit looking version by nearest-neighbor downscaling
        // 128 -> 16 -> 128 gives big readable pixels with no smoothing
        private static Icon Pixelate(Icon source)
        {
            const int CELL_SIZE = 16;

            try
            {
                using var bmp = source.ToBitmap();
                int size = Math.Min(bmp.Width, bmp.Height);
                int cells = size / CELL_SIZE;
                if (cells < 2) return source;

                using var small = new Bitmap(cells, cells);
                using (var g = Graphics.FromImage(small))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(bmp, new Rectangle(0, 0, cells, cells));
                }

                using var big = new Bitmap(size, size);
                using (var g = Graphics.FromImage(big))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(small, new Rectangle(0, 0, size, size));
                }

                IntPtr handle = big.GetHicon();
                try
                {
                    return (Icon)Icon.FromHandle(handle).Clone();
                }
                finally
                {
                    DestroyIcon(handle);
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("BootstrapperIconEx::Pixelate", ex);
                return source;
            }
        }
    }
}
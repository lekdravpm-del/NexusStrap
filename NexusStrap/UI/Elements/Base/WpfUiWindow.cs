using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace NexusStrap.UI.Elements.Base
{
    public abstract class WpfUiWindow : UiWindow
    {
        // I could add animated backgrounds, its easy but its hella gay i need to add a seperate image control :/
        private readonly IThemeService _themeService = new ThemeService();

        public WpfUiWindow()
        {
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            const int customThemeIndex = 2;

            var finalTheme = App.Settings.Prop.Theme.GetFinal();

            _themeService.SetTheme(finalTheme == Enums.Theme.Light ? ThemeType.Light : ThemeType.Dark);
            _themeService.SetSystemAccent();

            Application.Current.Resources["ApplicationBackground"] = null;

            if (finalTheme == Enums.Theme.Dark || finalTheme == Enums.Theme.Default)
            {
                ApplyDarkAccent();
            }

            if (finalTheme == Enums.Theme.NexusStrap)
            {
                ApplyNexusStrapAccent();
                ApplyNexusStrapBackground();
                this.WindowBackdropType = BackgroundType.None;
                this.Background = new SolidColorBrush(Colors.Black);
            }
            else if (finalTheme == Enums.Theme.Femboy)
            {
                ApplyFemboyAccent();
                this.WindowBackdropType = BackgroundType.None;
            }
            else
            {
                this.Background = null;
            }

            if (finalTheme == Enums.Theme.Custom)
            {
                if (App.Settings.Prop.BackgroundType == BackgroundMode.Gradient)
                {
                    ApplyGradientBackground();
                }
                else if (App.Settings.Prop.BackgroundType == BackgroundMode.Image)
                {
                    ApplyImageBackground();
                }

                ApplyCustomThemeResources();
            }
            else
            {
                ApplyStandardTheme(finalTheme, customThemeIndex);
            }

#if QA_BUILD
    this.BorderBrush = System.Windows.Media.Brushes.Red;
    this.BorderThickness = new Thickness(4);
#endif
        }

        private void ApplyNexusStrapAccent()
        {
            var white = Colors.White;
            var blackText = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);

            Application.Current.Resources["SystemAccentColor"] = white;
            Application.Current.Resources["SystemAccentColorPrimary"] = white;
            Application.Current.Resources["SystemAccentColorSecondary"] = white;
            Application.Current.Resources["SystemAccentColorTertiary"] = white;
            Application.Current.Resources["SystemAccentBrush"] = new SolidColorBrush(white);
            Application.Current.Resources["SystemFillColorAttentionBrush"] = new SolidColorBrush(white);
            Application.Current.Resources["AccentFillColorDefaultBrush"] = new SolidColorBrush(white);
            Application.Current.Resources["AccentFillColorSecondaryBrush"] = new SolidColorBrush(white);
            Application.Current.Resources["AccentFillColorTertiaryBrush"] = new SolidColorBrush(white);
            Application.Current.Resources["AccentFillColorSelectedTextBackgroundBrush"] = new SolidColorBrush(white);
            Application.Current.Resources["AccentTextFillColorPrimaryBrush"] = new SolidColorBrush(blackText);
            Application.Current.Resources["AccentTextFillColorSecondaryBrush"] = new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0x00, 0x00));
            Application.Current.Resources["AccentTextFillColorTertiaryBrush"] = new SolidColorBrush(Color.FromArgb(0x66, 0x00, 0x00, 0x00));
            Application.Current.Resources["TextOnAccentFillColorPrimary"] = blackText;
            Application.Current.Resources["TextOnAccentFillColorSecondary"] = Color.FromArgb(0x80, 0x00, 0x00, 0x00);
        }

        private void ApplyFemboyAccent()
        {
            var pink = Color.FromRgb(0xFF, 0x8F, 0xAB);
            var pinkDeep = Color.FromRgb(0xEC, 0x6E, 0x94);
            var whiteText = Colors.White;

            Application.Current.Resources["SystemAccentColor"] = pink;
            Application.Current.Resources["SystemAccentColorPrimary"] = pink;
            Application.Current.Resources["SystemAccentColorSecondary"] = pinkDeep;
            Application.Current.Resources["SystemAccentColorTertiary"] = pinkDeep;
            Application.Current.Resources["SystemAccentBrush"] = new SolidColorBrush(pink);
            Application.Current.Resources["SystemFillColorAttentionBrush"] = new SolidColorBrush(pink);
            Application.Current.Resources["AccentFillColorDefaultBrush"] = new SolidColorBrush(pink);
            Application.Current.Resources["AccentFillColorSecondaryBrush"] = new SolidColorBrush(pinkDeep);
            Application.Current.Resources["AccentFillColorTertiaryBrush"] = new SolidColorBrush(pinkDeep);
            Application.Current.Resources["AccentFillColorSelectedTextBackgroundBrush"] = new SolidColorBrush(pinkDeep);
            Application.Current.Resources["AccentTextFillColorPrimaryBrush"] = new SolidColorBrush(whiteText);
            Application.Current.Resources["AccentTextFillColorSecondaryBrush"] = new SolidColorBrush(Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF));
            Application.Current.Resources["AccentTextFillColorTertiaryBrush"] = new SolidColorBrush(Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF));
            Application.Current.Resources["TextOnAccentFillColorPrimary"] = whiteText;
            Application.Current.Resources["TextOnAccentFillColorSecondary"] = Color.FromArgb(0x80, 0xFF, 0xFF, 0xFF);
        }

        private void ApplyDarkAccent()
        {
            var black = Colors.Black;
            var whiteText = Colors.White;

            Application.Current.Resources["SystemAccentColor"] = black;
            Application.Current.Resources["SystemAccentColorPrimary"] = black;
            Application.Current.Resources["SystemAccentColorSecondary"] = black;
            Application.Current.Resources["SystemAccentColorTertiary"] = black;
            Application.Current.Resources["SystemAccentBrush"] = new SolidColorBrush(black);
            Application.Current.Resources["SystemFillColorAttentionBrush"] = new SolidColorBrush(black);
            Application.Current.Resources["AccentFillColorDefaultBrush"] = new SolidColorBrush(black);
            Application.Current.Resources["AccentFillColorSecondaryBrush"] = new SolidColorBrush(black);
            Application.Current.Resources["AccentFillColorTertiaryBrush"] = new SolidColorBrush(black);
            Application.Current.Resources["AccentFillColorSelectedTextBackgroundBrush"] = new SolidColorBrush(black);
            Application.Current.Resources["AccentTextFillColorPrimaryBrush"] = new SolidColorBrush(whiteText);
            Application.Current.Resources["AccentTextFillColorSecondaryBrush"] = new SolidColorBrush(Colors.White);
            Application.Current.Resources["AccentTextFillColorTertiaryBrush"] = new SolidColorBrush(Colors.White);
            Application.Current.Resources["TextOnAccentFillColorPrimary"] = whiteText;
            Application.Current.Resources["TextOnAccentFillColorSecondary"] = Colors.White;
        }

        private void ApplyNexusStrapBackground()
        {
            var black = new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0x00));
            black.Freeze();

            var fog = new LinearGradientBrush
            {
                StartPoint = new Point(1, 1),
                EndPoint = new Point(0, 0)
            };
            fog.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x00, 0x00, 0x00), 0.00));
            fog.GradientStops.Add(new GradientStop(Color.FromArgb(0x0C, 0xFF, 0xFF, 0xFF), 0.30));
            fog.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x0D, 0x0D, 0x0D), 0.48));
            fog.GradientStops.Add(new GradientStop(Color.FromArgb(0x0C, 0xFF, 0xFF, 0xFF), 0.62));
            fog.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x00, 0x00, 0x00), 1.00));
            fog.Freeze();

            Application.Current.Resources["ApplicationBackground"] = fog;
            Application.Current.Resources["ApplicationBackgroundBrush"] = black;
        }

        private void ApplyGradientBackground()
        {
            double angle = App.Settings.Prop.GradientAngle;
            double angleRad = angle * Math.PI / 180.0;

            double startX = 0.5 + 0.5 * Math.Cos(angleRad + Math.PI);
            double startY = 0.5 + 0.5 * Math.Sin(angleRad + Math.PI);
            double endX = 0.5 + 0.5 * Math.Cos(angleRad);
            double endY = 0.5 + 0.5 * Math.Sin(angleRad);

            var customBrush = new LinearGradientBrush
            {
                StartPoint = new Point(startX, startY),
                EndPoint = new Point(endX, endY)
            };

            foreach (var stop in App.Settings.Prop.CustomGradientStops.OrderBy(s => s.Offset))
            {
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(stop.Color);
                    customBrush.GradientStops.Add(new GradientStop(color, stop.Offset));
                }
                catch (Exception ex)
                {
                    App.Logger.WriteException("WpfUiWindow::ApplyGradientBackground", ex);
                }
            }

            Application.Current.Resources["ApplicationBackground"] = customBrush;
        }

        private void ApplyImageBackground()
        {
            if (string.IsNullOrEmpty(App.Settings.Prop.BackgroundImagePath) || !File.Exists(App.Settings.Prop.BackgroundImagePath))
            {
                return;
            }

            try
            {
                var imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.CacheOption = BitmapCacheOption.OnLoad;
                imageSource.UriSource = new Uri(App.Settings.Prop.BackgroundImagePath);
                imageSource.EndInit();
                imageSource.Freeze();

                var imageBrush = new ImageBrush
                {
                    ImageSource = imageSource,
                    Stretch = App.Settings.Prop.BackgroundStretch switch
                    {
                        BackgroundStretch.None => Stretch.None,
                        BackgroundStretch.Fill => Stretch.Fill,
                        BackgroundStretch.Uniform => Stretch.Uniform,
                        BackgroundStretch.UniformToFill => Stretch.UniformToFill,
                        _ => Stretch.UniformToFill
                    },
                    Opacity = App.Settings.Prop.BackgroundOpacity
                };

                Application.Current.Resources["ApplicationBackground"] = imageBrush;
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine("WpfUiWindow", $"Exception when changing to image: {ex.Message}");
            }
        }

        private void ApplyCustomThemeResources()
        {
            Application.Current.Resources["NewTextEditorBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#59000000"));
            Application.Current.Resources["NewTextEditorForeground"] = new SolidColorBrush(Colors.White);
            Application.Current.Resources["NewTextEditorLink"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3A9CEA"));
            Application.Current.Resources["PrimaryBackgroundColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#19000000"));
            Application.Current.Resources["NormalDarkAndLightBackground"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0FFFFFFF"));
            Application.Current.Resources["ControlFillColorDefault"] = (Color)ColorConverter.ConvertFromString("#19000000");
        }

        private void ApplyStandardTheme(Enums.Theme finalTheme, int customThemeIndex)
        {
            var dict = new ResourceDictionary { Source = new Uri($"pack://application:,,,/UI/Style/{Enum.GetName(finalTheme)}.xaml") };
            Application.Current.Resources.MergedDictionaries[customThemeIndex] = dict;

            Application.Current.Resources.Remove("NewTextEditorBackground");
            Application.Current.Resources.Remove("NewTextEditorForeground");
            Application.Current.Resources.Remove("NewTextEditorLink");
            Application.Current.Resources.Remove("PrimaryBackgroundColor");
            Application.Current.Resources.Remove("NormalDarkAndLightBackground");
            Application.Current.Resources.Remove("ControlFillColorDefault");
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Hardware Accel
            if (App.Settings.Prop.WPFSoftwareRender || App.LaunchSettings.NoGPUFlag.Active)
            {
                if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
                    hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
            }

            // Custom Font
            string? fontPath = App.Settings.Prop.CustomFontPath;
            if (!string.IsNullOrWhiteSpace(fontPath) && File.Exists(fontPath))
            {
                var font = FontManager.LoadFontFromFile(fontPath);
                if (font != null)
                {
                    this.FontFamily = font;
                }
            }
        }
    }
}
using System.Windows;
using System.Windows.Media;

namespace NexusStrap.UI
{
    public static class RoundedWindowChrome
    {
        public const double CornerRadius = 8.0;

        private static bool _installed;

        public static void Install()
        {
            if (_installed)
                return;

            _installed = true;
            EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnWindowLoaded));
        }

        private static void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Window window)
                return;

            ApplyClip(window);
            window.SizeChanged -= OnWindowSizeChanged;
            window.SizeChanged += OnWindowSizeChanged;
            window.StateChanged -= OnWindowStateChanged;
            window.StateChanged += OnWindowStateChanged;
            window.Closed -= OnWindowClosed;
            window.Closed += OnWindowClosed;
        }

        private static void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            ApplyClip(sender as Window);
        }

        private static void OnWindowStateChanged(object? sender, System.EventArgs e)
        {
            ApplyClip(sender as Window);
        }

        private static void OnWindowClosed(object? sender, System.EventArgs e)
        {
            if (sender is not Window window)
                return;

            window.SizeChanged -= OnWindowSizeChanged;
            window.StateChanged -= OnWindowStateChanged;
            window.Closed -= OnWindowClosed;
        }

        private static void ApplyClip(Window? window)
        {
            if (window == null)
                return;

            if (!window.AllowsTransparency)
            {
                window.Clip = null;
                return;
            }

            double width = window.ActualWidth;
            double height = window.ActualHeight;

            if (window.WindowState == System.Windows.WindowState.Maximized || width <= 0.0 || height <= 0.0)
            {
                window.Clip = null;
                return;
            }

            var geometry = new RectangleGeometry(new Rect(0.0, 0.0, width, height), CornerRadius, CornerRadius);
            geometry.Freeze();
            window.Clip = geometry;
        }
    }
}
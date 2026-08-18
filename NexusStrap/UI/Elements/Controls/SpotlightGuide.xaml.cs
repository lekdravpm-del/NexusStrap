using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NexusStrap.UI.Elements.Settings.Pages;
using Wpf.Ui.Controls;

namespace NexusStrap.UI.Elements.Controls
{
    public partial class SpotlightGuide : UserControl
    {
        private sealed class GuideStep
        {
            public string Section = "";
            public string Title = "";
            public string Description = "";
            public string? NavTag;
            public Type? PageType;
            public string? ElementName;
            public bool IsSectionIntro => NavTag != null;
        }

        private Settings.MainWindow? _window;
        private readonly List<GuideStep> _steps = new();
        private int _stepIndex;
        private bool _running;
        private bool _transitioning;
        private Type? _originalPage;
        private Rect _currentHole = new(0, 0, 0, 0);
        private readonly RectangleGeometry _fullRect = new();
        private readonly RectangleGeometry _holeRect = new();

        public event EventHandler? GuideCompleted;

        public SpotlightGuide()
        {
            InitializeComponent();

            SizeChanged += (_, _) => UpdateFullRect();

            InitLanguageCombo();
        }

        private void InitLanguageCombo()
        {
            var languages = Locale.GetLanguages();
            LanguageCombo.ItemsSource = languages;

            var currentId = App.Settings.Prop.Locale;
            var currentName = Locale.SupportedLocales.TryGetValue(currentId, out var name) ? name : null;
            if (currentName != null)
                LanguageCombo.SelectedItem = currentName;
            else
                LanguageCombo.SelectedIndex = 0;
        }

        private void LanguageCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageCombo.SelectedItem is not string selectedName) return;

            var identifier = Locale.GetIdentifierFromName(selectedName);
            if (identifier == App.Settings.Prop.Locale) return;

            Locale.Set(identifier);
            App.Settings.Prop.Locale = identifier;
            App.Settings.Save();

            UpdateWelcomeText();

            if (_running && !_transitioning && _stepIndex < _steps.Count)
            {
                _steps.Clear();
                BuildSteps();
                if (_stepIndex >= _steps.Count) _stepIndex = _steps.Count - 1;
                _ = ShowStepAsync(_stepIndex);
            }
        }

        private void UpdateWelcomeText()
        {
            WelcomeTitle.Text = "Welcome to NexusStrap!";
            WelcomeSubtitle.Text = "Thanks for choosing NexusStrap. Let's walk you through the settings.";
            WelcomeCredit.Text = "Made by NexusStrap Contributors";
            WelcomeQuestion.Text = "Would you like a quick tour?";
            WelcomeSkipBtn.Content = "Skip";
            WelcomeStartBtn.Content = "Start Tour";
            StepHint.Text = "Click anywhere to continue";
            BackBtn.Content = "Back";
            SkipBtn.Content = "Skip Tour";
        }

        public void Start(Settings.MainWindow window)
        {
            _window = window;
            _originalPage = window.GetFrame().Content?.GetType();

            _steps.Clear();
            BuildSteps();
            _stepIndex = 0;
            _running = false;
            _transitioning = false;

            Opacity = 1;
            Visibility = Visibility.Visible;
            WelcomePanel.Visibility = Visibility.Visible;
            StepCard.Visibility = Visibility.Collapsed;
            HighlightBox.Visibility = Visibility.Collapsed;

            UpdateWelcomeText();
            UpdateLayout();

            _fullRect.Rect = new Rect(0, 0, ActualWidth, ActualHeight);
            _holeRect.Rect = _fullRect.Rect;
            _currentHole = _fullRect.Rect;
            DimPath.Data = new CombinedGeometry(GeometryCombineMode.Exclude, _fullRect, _holeRect);
        }

        private void UpdateFullRect()
        {
            _fullRect.Rect = new Rect(0, 0, ActualWidth, ActualHeight);
            if (_holeRect.Rect.Width <= 0 || _holeRect.Rect.Height <= 0)
                _holeRect.Rect = _fullRect.Rect;
        }

        private void BuildSteps()
        {
            void Section(string navTag, Type pageType, string name, string sectionDescription,
                params (string Element, string Title, string Description)[] settings)
            {
                _steps.Add(new GuideStep
                {
                    Section = name,
                    Title = name,
                    Description = sectionDescription,
                    NavTag = navTag
                });

                foreach (var setting in settings)
                {
                    _steps.Add(new GuideStep
                    {
                        Section = name,
                        Title = setting.Title,
                        Description = setting.Description,
                        PageType = pageType,
                        ElementName = setting.Element
                    });
                }
            }

            Section("integrations", typeof(IntegrationsPage),
                "Integrations", "Manage activity tracking, Discord RPC, and custom integrations.",
                ("ActivityTrackingOption", "Integrations", "Enable activity tracking to show game info and server details."),
                ("DiscordActivityOption", "Integrations", "Show your current game on Discord."),
                ("StudioActivityOption", "Integrations", "Show Roblox Studio activity on Discord."),
                ("CustomIntegrationsListBox", "Integrations", "Add custom apps to launch with Roblox."));

            Section("bootstrapper", typeof(BehaviourPage),
                "Bootstrapper", "Configure how NexusStrap launches and behaves.",
                ("ConfirmLaunchesToggle", "Bootstrapper", "Confirm before each launch."),
                ("CookieAccessToggle", "Bootstrapper", "Allow cookie access for server features."),
                ("UncapFpsToggle", "Bootstrapper", "Remove the default FPS cap."),
                ("ProcessPriorityOption", "Bootstrapper", "Set Roblox process priority."),
                ("BackgroundUpdatesToggle", "Bootstrapper", "Enable silent background updates."));

            Section("fastflags", typeof(FastFlagsPage),
                "Fast Flags", "Override Roblox client flags for performance and visuals.",
                ("ManagerEnabled", "Fast Flags", "Enable the Fast Flag manager."),
                ("Reset", "Fast Flags", "Reset all flags to default."));

            Section("fflagtemplates", typeof(FFlagTemplatesPage),
                "FFlag Templates", "Apply pre-made flag configurations.",
                ("TemplateSearchBox", "FFlag Templates", "Search for flag templates."),
                ("TemplateImportButton", "FFlag Templates", "Import a custom template."),
                ("CategoryPerformance", "FFlag Templates", "Browse templates by category."));

            Section("mods", typeof(ModsPresetsPage),
                "Mods", "Customize your Roblox experience with mods.",
                ("OldAvatarEditorToggle", "Mods", "Revert to the old avatar editor."),
                ("CursorTypeOption", "Mods", "Choose a custom cursor style."),
                ("EmojiTypeOption", "Mods", "Change the emoji set."),
                ("CustomFontOption", "Mods", "Use a custom font in-game."),
                ("CustomDeathSoundOption", "Mods", "Change the death sound."),
                ("CustomCursorOption", "Mods", "Upload your own cursor."),
                ("CustomShiftlockOption", "Mods", "Upload a custom shift lock cursor."));

            Section("appearance", typeof(AppearancePage),
                "Appearance", "Customize the NexusStrap window look.",
                ("IconSelector", "Appearance", "Change the application icon."),
                ("CustomThemesListBox", "Appearance", "Create and apply custom themes."));

            Section("regionselector", typeof(RegionSelectorPage),
                "Region Selector", "Find and join servers in specific regions.",
                ("SearchComboBox", "Region Selector", "Search for a game."),
                ("SortOrderComboBox", "Region Selector", "Sort servers by different criteria."),
                ("RegionComboBox", "Region Selector", "Filter by server region."));

            Section("robloxsettings", typeof(RobloxSettingsPage),
                "Roblox Settings", "Manage your Roblox client settings.",
                ("ImportCard", "Roblox Settings", "Import settings from a file."),
                ("OpenFolderCard", "Roblox Settings", "Open the Roblox settings folder."),
                ("ExportCard", "Roblox Settings", "Export your settings."),
                ("ReadOnlyToggle", "Roblox Settings", "Protect settings from being overwritten."));

            Section("shortcuts", typeof(ShortcutsPage),
                "Shortcuts", "Manage desktop and start menu shortcuts.",
                ("ExtractIconsToggle", "Shortcuts", "Extract icons from Roblox."),
                ("DesktopIconToggle", "Shortcuts", "Create a desktop shortcut."),
                ("StartMenuIconToggle", "Shortcuts", "Create a start menu shortcut."),
                ("PlayerIconToggle", "Shortcuts", "Create a Roblox Player shortcut."),
                ("StudioIconToggle", "Shortcuts", "Create a Roblox Studio shortcut."),
                ("SettingsIconToggle", "Shortcuts", "Create a settings shortcut."));

            Section("logviewer", typeof(LogViewerPage),
                "Log Viewer", "View NexusStrap logs for debugging.",
                ("LogFilesComboBox", "Log Viewer", "Select a log file."),
                ("LogSearchTextBox", "Log Viewer", "Search through log entries."),
                ("AddArgButton", "Log Viewer", "Add custom launch arguments."));

            Section("appanalyzer", typeof(AppAnalyzerPage),
                "App Analyzer", "Scan your installation for issues.",
                ("ScanConflictsButton", "App Analyzer", "Scan for file conflicts."),
                ("RunHealthCheckButton", "App Analyzer", "Run a health check."));

            Section("analytics", typeof(AnalyticsPage),
                "Analytics", "View your usage statistics.",
                ("RefreshButton", "Analytics", "Refresh analytics data."));
        }

        private void StartTour_Click(object sender, RoutedEventArgs e)
        {
            WelcomePanel.Visibility = Visibility.Collapsed;
            _running = true;
            _ = ShowStepAsync(0);
        }

        private void Root_Click(object sender, MouseButtonEventArgs e)
        {
            if (WelcomePanel.Visibility == Visibility.Visible) return;
            if (!_running || _transitioning) return;
            Advance();
        }

        private void StepCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (!_running || _transitioning) return;
            Advance();
        }

        private async void Advance()
        {
            if (_stepIndex >= _steps.Count - 1)
            {
                Complete();
                return;
            }

            _stepIndex++;
            await ShowStepAsync(_stepIndex);
        }

        private async Task ShowStepAsync(int index)
        {
            _transitioning = true;

            try
            {
                var step = _steps[index];

                StepCounter.Text = $"{index + 1} / {_steps.Count}";
                StepSection.Text = step.Section;
                StepTitle.Text = step.Title;
                StepDescription.Text = step.Description;
                BackBtn.Visibility = index > 0 ? Visibility.Visible : Visibility.Collapsed;

                FrameworkElement? target = null;

                if (step.NavTag != null)
                {
                    target = FindNavItem(step.NavTag);
                }
                else if (step.PageType != null && step.ElementName != null)
                {
                    await NavigateToAsync(step.PageType);
                    target = FindPageElement(step.ElementName);
                }

                Rect hole;
                if (target != null && TryGetElementRect(target, out Rect rect))
                {
                    rect.Inflate(10, 10);
                    hole = rect;
                    HighlightBox.Visibility = Visibility.Visible;
                    PositionHighlight(rect);
                }
                else
                {
                    hole = new Rect(0, 0, ActualWidth, ActualHeight);
                    HighlightBox.Visibility = Visibility.Collapsed;
                }

                var anim = new RectAnimation(_currentHole, hole, TimeSpan.FromMilliseconds(280))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
                };
                _holeRect.BeginAnimation(RectangleGeometry.RectProperty, anim);
                _currentHole = hole;

                StepCard.Visibility = Visibility.Visible;
                PositionCard(target != null ? hole : Rect.Empty);
                StepCard.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)));
            }
            finally
            {
                _transitioning = false;
            }
        }

        private void StepBack_Click(object sender, RoutedEventArgs e)
        {
            if (!_running || _transitioning) return;
            if (_stepIndex <= 0) return;

            _stepIndex--;
            _ = ShowStepAsync(_stepIndex);
        }

        private void Skip_Click(object sender, RoutedEventArgs e)
        {
            CloseGuide();
        }

        private void CloseGuide()
        {
            if (!_running && WelcomePanel.Visibility != Visibility.Visible) return;
            _running = false;

            App.State.Prop.HasSeenGuide = true;
            App.State.Save();

            RestoreOriginalPage();

            var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fade.Completed += (_, _) =>
            {
                Visibility = Visibility.Collapsed;
                GuideCompleted?.Invoke(this, EventArgs.Empty);
            };
            BeginAnimation(OpacityProperty, fade);
        }

        private void Complete()
        {
            if (!_running && WelcomePanel.Visibility != Visibility.Visible) return;
            _running = false;

            App.State.Prop.HasSeenGuide = true;
            App.State.Save();

            RestoreOriginalPage();

            var fade = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fade.Completed += (_, _) =>
            {
                Visibility = Visibility.Collapsed;
                GuideCompleted?.Invoke(this, EventArgs.Empty);
            };
            BeginAnimation(OpacityProperty, fade);
        }

        private void RestoreOriginalPage()
        {
            if (_window == null || _originalPage == null) return;

            if (_originalPage == typeof(RobloxSettingsPage) && !App.GlobalSettings.Loaded)
                return;

            if (_window.GetFrame().Content?.GetType() != _originalPage)
                _window.Navigate(_originalPage);
        }

        private FrameworkElement? FindNavItem(string tag)
        {
            if (_window == null) return null;

            foreach (var item in _window.RootNavigation.Items)
            {
                if (item is NavigationItem navItem && navItem.Tag is string itemTag && itemTag == tag)
                    return navItem;
            }

            return null;
        }

        private FrameworkElement? FindPageElement(string name)
        {
            if (_window == null) return null;
            if (_window.GetFrame().Content is not FrameworkElement page) return null;
            return FindByName(page, name);
        }

        private static FrameworkElement? FindByName(DependencyObject root, string name)
        {
            if (root is FrameworkElement fe && fe.Name == name) return fe;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var result = FindByName(VisualTreeHelper.GetChild(root, i), name);
                if (result != null) return result;
            }

            return null;
        }

        private async Task NavigateToAsync(Type pageType)
        {
            if (_window == null) return;
            if (_window.GetFrame().Content?.GetType() == pageType) return;

            _window.Navigate(pageType);

            for (int i = 0; i < 60; i++)
            {
                await Task.Delay(50);
                if (_window.GetFrame().Content?.GetType() == pageType) return;
            }
        }

        private bool TryGetElementRect(FrameworkElement element, out Rect rect)
        {
            rect = default;

            try
            {
                element.BringIntoView();

                element.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
                element.UpdateLayout();

                var transform = element.TransformToVisual(this);
                if (transform == null) return false;

                var topLeft = transform.Transform(new Point(0, 0));

                double w = element.ActualWidth;
                double h = element.ActualHeight;

                if (w <= 0 || h <= 0)
                {
                    w = element.RenderSize.Width;
                    h = element.RenderSize.Height;
                }

                if (w <= 0 || h <= 0) return false;

                rect = new Rect(topLeft.X, topLeft.Y, w, h);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void PositionHighlight(Rect r)
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseInOut };

            double fromLeft = Canvas.GetLeft(HighlightBox);
            double fromTop = Canvas.GetTop(HighlightBox);
            double fromWidth = HighlightBox.ActualWidth;
            double fromHeight = HighlightBox.ActualHeight;

            if (double.IsNaN(fromLeft)) fromLeft = 0;
            if (double.IsNaN(fromTop)) fromTop = 0;
            if (fromWidth <= 0 || double.IsNaN(fromWidth)) fromWidth = r.Width;
            if (fromHeight <= 0 || double.IsNaN(fromHeight)) fromHeight = r.Height;

            HighlightBox.BeginAnimation(Canvas.LeftProperty, new DoubleAnimation(fromLeft, r.X, TimeSpan.FromMilliseconds(280)) { EasingFunction = ease });
            HighlightBox.BeginAnimation(Canvas.TopProperty, new DoubleAnimation(fromTop, r.Y, TimeSpan.FromMilliseconds(280)) { EasingFunction = ease });
            HighlightBox.BeginAnimation(WidthProperty, new DoubleAnimation(fromWidth, r.Width, TimeSpan.FromMilliseconds(280)) { EasingFunction = ease });
            HighlightBox.BeginAnimation(HeightProperty, new DoubleAnimation(fromHeight, r.Height, TimeSpan.FromMilliseconds(280)) { EasingFunction = ease });
        }

        private void PositionCard(Rect hole)
        {
            StepCard.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            StepCard.UpdateLayout();

            double cw = StepCard.ActualWidth;
            double ch = StepCard.ActualHeight;
            const double gap = 14;
            const double pad = 16;

            double x, y;

            if (hole.IsEmpty)
            {
                x = Math.Max(pad, (ActualWidth - cw) / 2);
                y = Math.Max(pad, (ActualHeight - ch) / 2);
            }
            else
            {
                // Try right side first, aligned to highlight top
                if (hole.Right + gap + cw <= ActualWidth - pad)
                {
                    x = hole.Right + gap;
                    y = hole.Top;
                }
                // Try left side, aligned to highlight top
                else if (hole.Left - gap - cw >= pad)
                {
                    x = hole.Left - gap - cw;
                    y = hole.Top;
                }
                // Try below, aligned to highlight left
                else if (hole.Bottom + gap + ch <= ActualHeight - pad)
                {
                    x = hole.Left;
                    y = hole.Bottom + gap;
                }
                // Try above, aligned to highlight left
                else if (hole.Top - gap - ch >= pad)
                {
                    x = hole.Left;
                    y = hole.Top - gap - ch;
                }
                // Fallback: center on screen
                else
                {
                    x = Math.Max(pad, (ActualWidth - cw) / 2);
                    y = Math.Max(pad, (ActualHeight - ch) / 2);
                }

                // Clamp to screen bounds
                x = Math.Max(pad, Math.Min(ActualWidth - cw - pad, x));
                y = Math.Max(pad, Math.Min(ActualHeight - ch - pad, y));
            }

            Canvas.SetLeft(StepCard, x);
            Canvas.SetTop(StepCard, y);
        }
    }
}

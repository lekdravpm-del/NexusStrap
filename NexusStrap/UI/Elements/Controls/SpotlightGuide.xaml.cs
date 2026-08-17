using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NexusStrap.Resources;
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
            WelcomeTitle.Text = Strings.Guide_Welcome_Hello;
            WelcomeSubtitle.Text = Strings.Guide_Welcome_Thanks;
            WelcomeCredit.Text = Strings.Guide_Welcome_Credit;
            WelcomeQuestion.Text = Strings.Guide_Welcome_Question;
            WelcomeSkipBtn.Content = Strings.Guide_Welcome_Skip;
            WelcomeStartBtn.Content = Strings.Guide_Welcome_StartTour;
            StepHint.Text = Strings.Guide_Step_ClickToContinue;
            BackBtn.Content = Strings.Guide_Step_Back;
            SkipBtn.Content = Strings.Guide_Step_SkipTour;
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
                Strings.Guide_Section_Integrations,
                Strings.Guide_Section_Integrations_Description,
                ("ActivityTrackingOption", Strings.Guide_Section_Integrations, Strings.Guide_Step_Integrations_ActivityTracking),
                ("DiscordActivityOption", Strings.Guide_Section_Integrations, Strings.Guide_Step_Integrations_DiscordActivity),
                ("StudioActivityOption", Strings.Guide_Section_Integrations, Strings.Guide_Step_Integrations_StudioActivity),
                ("CustomIntegrationsListBox", Strings.Guide_Section_Integrations, Strings.Guide_Step_Integrations_CustomIntegrations));

            Section("bootstrapper", typeof(BehaviourPage),
                Strings.Guide_Section_Bootstrapper,
                Strings.Guide_Section_Bootstrapper_Description,
                ("ConfirmLaunchesToggle", Strings.Guide_Section_Bootstrapper, Strings.Guide_Step_Bootstrapper_ConfirmLaunches),
                ("CookieAccessToggle", Strings.Guide_Section_Bootstrapper, Strings.Guide_Step_Bootstrapper_CookieAccess),
                ("UncapFpsToggle", Strings.Guide_Section_Bootstrapper, Strings.Guide_Step_Bootstrapper_UncapFps),
                ("ProcessPriorityOption", Strings.Guide_Section_Bootstrapper, Strings.Guide_Step_Bootstrapper_ProcessPriority),
                ("BackgroundUpdatesToggle", Strings.Guide_Section_Bootstrapper, Strings.Guide_Step_Bootstrapper_BackgroundUpdates));

            Section("fastflags", typeof(FastFlagsPage),
                Strings.Guide_Section_FastFlags,
                Strings.Guide_Section_FastFlags_Description,
                ("ManagerEnabled", Strings.Guide_Section_FastFlags, Strings.Guide_Step_FastFlags_ManagerEnabled),
                ("Reset", Strings.Guide_Section_FastFlags, Strings.Guide_Step_FastFlags_Reset));

            Section("fflagtemplates", typeof(FFlagTemplatesPage),
                Strings.Guide_Section_FFlagTemplates,
                Strings.Guide_Section_FFlagTemplates_Description,
                ("TemplateSearchBox", Strings.Guide_Section_FFlagTemplates, Strings.Guide_Step_FFlagTemplates_Search),
                ("TemplateImportButton", Strings.Guide_Section_FFlagTemplates, Strings.Guide_Step_FFlagTemplates_Import),
                ("CategoryPerformance", Strings.Guide_Section_FFlagTemplates, Strings.Guide_Step_FFlagTemplates_Categories));

            Section("mods", typeof(ModsPresetsPage),
                Strings.Guide_Section_Mods,
                Strings.Guide_Section_Mods_Description,
                ("OldAvatarEditorToggle", Strings.Guide_Section_Mods, Strings.Guide_Step_Mods_OldAvatarEditor),
                ("OldCharacterSoundsToggle", Strings.Guide_Section_Mods, Strings.Guide_Step_Mods_OldCharacterSounds),
                ("OldJumpSoundToggle", Strings.Guide_Section_Mods, Strings.Guide_Step_Mods_OldJumpSound),
                ("SilenceFallingToggle", Strings.Guide_Section_Mods, Strings.Guide_Step_Mods_SilenceFalling),
                ("SilenceSwimToggle", Strings.Guide_Section_Mods, Strings.Guide_Step_Mods_SilenceSwim),
                ("CursorTypeOption", Strings.Guide_Section_Mods, Strings.Guide_Step_Mods_CursorType),
                ("EmojiTypeOption", Strings.Guide_Section_Mods, Strings.Guide_Step_Mods_EmojiType),
                ("CustomFontOption", Strings.Guide_Section_Mods, Strings.Guide_Step_Mods_CustomFont),
                ("CustomDeathSoundOption", Strings.Guide_Section_Mods, Strings.Guide_Step_Mods_CustomDeathSound),
                ("CustomCursorOption", Strings.Guide_Section_Mods, Strings.Guide_Step_Mods_CustomCursor),
                ("CustomShiftlockOption", Strings.Guide_Section_Mods, Strings.Guide_Step_Mods_CustomShiftlock));

            Section("appearance", typeof(AppearancePage),
                Strings.Guide_Section_Appearance,
                Strings.Guide_Section_Appearance_Description,
                ("IconSelector", Strings.Guide_Section_Appearance, Strings.Guide_Step_Appearance_IconSelector),
                ("CustomThemesListBox", Strings.Guide_Section_Appearance, Strings.Guide_Step_Appearance_CustomThemes));

            Section("regionselector", typeof(RegionSelectorPage),
                Strings.Guide_Section_RegionSelector,
                Strings.Guide_Section_RegionSelector_Description,
                ("SearchComboBox", Strings.Guide_Section_RegionSelector, Strings.Guide_Step_RegionSelector_Search),
                ("SortOrderComboBox", Strings.Guide_Section_RegionSelector, Strings.Guide_Step_RegionSelector_SortOrder),
                ("RegionComboBox", Strings.Guide_Section_RegionSelector, Strings.Guide_Step_RegionSelector_Region));

            Section("robloxsettings", typeof(RobloxSettingsPage),
                Strings.Guide_Section_RobloxSettings,
                Strings.Guide_Section_RobloxSettings_Description,
                ("ImportCard", Strings.Guide_Section_RobloxSettings, Strings.Guide_Step_RobloxSettings_ImportCard),
                ("OpenFolderCard", Strings.Guide_Section_RobloxSettings, Strings.Guide_Step_RobloxSettings_OpenFolderCard),
                ("ExportCard", Strings.Guide_Section_RobloxSettings, Strings.Guide_Step_RobloxSettings_ExportCard),
                ("ReadOnlyToggle", Strings.Guide_Section_RobloxSettings, Strings.Guide_Step_RobloxSettings_ReadOnlyToggle));

            Section("shortcuts", typeof(ShortcutsPage),
                Strings.Guide_Section_Shortcuts,
                Strings.Guide_Section_Shortcuts_Description,
                ("ExtractIconsToggle", Strings.Guide_Section_Shortcuts, Strings.Guide_Step_Shortcuts_ExtractIcons),
                ("DesktopIconToggle", Strings.Guide_Section_Shortcuts, Strings.Guide_Step_Shortcuts_DesktopIcon),
                ("StartMenuIconToggle", Strings.Guide_Section_Shortcuts, Strings.Guide_Step_Shortcuts_StartMenuIcon),
                ("PlayerIconToggle", Strings.Guide_Section_Shortcuts, Strings.Guide_Step_Shortcuts_PlayerIcon),
                ("StudioIconToggle", Strings.Guide_Section_Shortcuts, Strings.Guide_Step_Shortcuts_StudioIcon),
                ("SettingsIconToggle", Strings.Guide_Section_Shortcuts, Strings.Guide_Step_Shortcuts_SettingsIcon));

            Section("logviewer", typeof(LogViewerPage),
                Strings.Guide_Section_LogViewer,
                Strings.Guide_Section_LogViewer_Description,
                ("LogFilesComboBox", Strings.Guide_Section_LogViewer, Strings.Guide_Step_LogViewer_LogFiles),
                ("LogSearchTextBox", Strings.Guide_Section_LogViewer, Strings.Guide_Step_LogViewer_SearchLogs),
                ("AddArgButton", Strings.Guide_Section_LogViewer, Strings.Guide_Step_LogViewer_AddArgument));

            Section("appanalyzer", typeof(AppAnalyzerPage),
                Strings.Guide_Section_AppAnalyzer,
                Strings.Guide_Section_AppAnalyzer_Description,
                ("ScanConflictsButton", Strings.Guide_Section_AppAnalyzer, Strings.Guide_Step_AppAnalyzer_ScanConflicts),
                ("RunHealthCheckButton", Strings.Guide_Section_AppAnalyzer, Strings.Guide_Step_AppAnalyzer_RunHealthCheck));

            Section("analytics", typeof(AnalyticsPage),
                Strings.Guide_Section_Analytics,
                Strings.Guide_Section_Analytics_Description,
                ("RefreshButton", Strings.Guide_Section_Analytics, Strings.Guide_Step_Analytics_Refresh));
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
            Complete();
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

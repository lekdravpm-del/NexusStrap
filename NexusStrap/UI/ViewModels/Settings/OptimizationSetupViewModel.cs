using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

using NexusStrap.Models;
using NexusStrap.UI.Elements.Settings.Pages;

using Wpf.Ui.Mvvm.Contracts;

namespace NexusStrap.UI.ViewModels.Settings
{
    internal class OptimizationSetupViewModel : NotifyPropertyChangedViewModel
    {
        private readonly Page _page;

        public HardwareInfo Hardware { get; }

        public string TierName { get; }

        public string RecommendedSettingsSummary { get; }

        public bool IsApplying { get; private set; }

        public ICommand ApplyCommand { get; }

        public ICommand SkipCommand { get; }

        public OptimizationSetupViewModel(Page page)
        {
            _page = page;

            ApplyCommand = new RelayCommand(Apply);
            SkipCommand = new RelayCommand(Skip);

            Hardware = HardwareInfo.Detect();
            TierName = Hardware.GetTierName();

            RecommendedSettingsSummary = Hardware.GetTier() switch
            {
                PerformanceTier.Ultra => "Full anti-aliasing (MSAA x4) and maximum graphics quality.",
                PerformanceTier.High => "Anti-aliasing (MSAA x4) and high graphics quality.",
                PerformanceTier.Mid => "Light anti-aliasing (MSAA x2) and balanced graphics quality.",
                _ => "Performance first: MSAA off, reduced grass, low-poly meshes and lowered graphics quality."
            };
        }

        private void Apply()
        {
            if (IsApplying)
                return;

            IsApplying = true;
            OnPropertyChanged(nameof(IsApplying));

            try
            {
                PerformanceOptimizer.Apply(Hardware.GetTier());

                App.State.Prop.ShowOptimizationSetup = false;
                App.State.Save();
            }
            finally
            {
                IsApplying = false;
                OnPropertyChanged(nameof(IsApplying));
            }

            NavigateAway();
        }

        private void Skip()
        {
            App.State.Prop.ShowOptimizationSetup = false;
            App.State.Save();

            NavigateAway();
        }

        private void NavigateAway()
        {
            if (Window.GetWindow(_page) is INavigationWindow window)
                window.Navigate(typeof(IntegrationsPage));
        }
    }
}
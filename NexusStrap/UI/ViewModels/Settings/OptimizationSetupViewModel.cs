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

            string gpuInfo = Hardware.GpuName != "Unknown" ? Hardware.GpuName : "Unknown GPU";
            string cpuInfo = Hardware.CpuName != "Unknown" ? Hardware.CpuName : "Unknown CPU";
            string ramInfo = $"{Hardware.TotalRamGB} GB RAM";
            string vramInfo = Hardware.GpuVramGB > 0 ? $" ({Hardware.GpuVramGB} GB VRAM)" : "";

            string gpuRecommendation = Hardware.GetTier() switch
            {
                PerformanceTier.Ultra => "Vulkan recommended for best raytracing support.",
                PerformanceTier.High => "Vulkan recommended for best performance.",
                PerformanceTier.Mid => "DirectX 11 (default) recommended for stability.",
                _ => "DirectX 11 recommended. Consider lowering resolution."
            };

            RecommendedSettingsSummary = $"Detected: {gpuInfo}{vramInfo}, {cpuInfo}, {ramInfo}\n" +
                $"Tier: {TierName}\n" +
                $"{gpuRecommendation}\n\n" +
                Hardware.GetTier() switch
                {
                    PerformanceTier.Ultra => "Will apply: MSAA x4, max graphics quality, all effects enabled.",
                    PerformanceTier.High => "Will apply: MSAA x4, high graphics quality, grass enabled.",
                    PerformanceTier.Mid => "Will apply: MSAA x2, balanced quality, grass enabled.",
                    _ => "Will apply: MSAA off, no grass, low-poly meshes, paused voxelizer."
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
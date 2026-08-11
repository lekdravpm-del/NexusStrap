using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NexusStrap.UI.ViewModels.Settings
{
    public class ModConflictViewModel : NotifyPropertyChangedViewModel
    {
        public ObservableCollection<string> Conflicts { get; set; } = new();
        public ICommand ScanCommand { get; }

        private string _statusText = "Click Scan to check for flag conflicts.";
        public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(nameof(StatusText)); } }

        private int _conflictCount;
        public int ConflictCount { get => _conflictCount; set { _conflictCount = value; OnPropertyChanged(nameof(ConflictCount)); } }

        private static readonly (string FlagA, string FlagB, string Reason)[] KnownConflicts = new[]
        {
            ("FFlagDebugGraphicsPreferVulkan", "FFlagDebugGraphicsPreferD3D11", "Cannot use both Vulkan and D3D11 renderers simultaneously"),
            ("FFlagDebugGraphicsPreferVulkan", "FFlagDebugGraphicsPreferD3D12", "Cannot use both Vulkan and D3D12 renderers simultaneously"),
            ("FFlagDebugGraphicsPreferD3D11", "FFlagDebugGraphicsPreferD3D12", "Cannot use both D3D11 and D3D12 renderers simultaneously"),
            ("DFFlagDisableDPIScale", "DFFlagEnableDPIScale", "Conflicting DPI scale settings"),
            ("FFlagDebugGraphicsDisableDirect3D11", "FFlagDebugGraphicsPreferD3D11", "D3D11 is both disabled and preferred"),
            ("FIntDebugFRMQualityLevelOverride", "FFlagDebugFRMQualityLevelOverride", "Duplicate frame quality settings"),
            ("DFIntTaskSchedulerTargetFps", "FIntDebugFRMQualityLevelOverride", "FPS cap and quality override may conflict"),
            ("FFlagDebugRenderingSignalCrit", "FFlagDebugForceFSMCPULoopbackSignalCrit", "Conflicting debug signal settings"),
            ("DFFlagDebugDrawCircle", "DFFlagDebugDrawSphere", "Multiple debug draw overrides active"),
            ("DFStringDebugConfigAvatarUniverseId", "DFStringDebugConfigAvatarPlaceId", "Avatar config may override game-specific settings"),
        };

        public ModConflictViewModel()
        {
            ScanCommand = new RelayCommand(ScanConflicts);
        }

        private void ScanConflicts()
        {
            Conflicts.Clear();
            ConflictCount = 0;

            var allFlags = App.FastFlags.GetAllFlags().ToDictionary(f => f.Name, f => f.Value);

            foreach (var (flagA, flagB, reason) in KnownConflicts)
            {
                if (allFlags.ContainsKey(flagA) && allFlags.ContainsKey(flagB))
                {
                    Conflicts.Add($"⚠ {flagA} ↔ {flagB}: {reason}");
                    ConflictCount++;
                }
            }

            var flagKeys = allFlags.Keys.ToList();
            for (int i = 0; i < flagKeys.Count; i++)
            {
                for (int j = i + 1; j < flagKeys.Count; j++)
                {
                    var a = flagKeys[i];
                    var b = flagKeys[j];
                    if (a.StartsWith("DFInt") && b.StartsWith("DFInt") && a.Replace("DFInt", "") == b.Replace("DFInt", ""))
                    {
                        Conflicts.Add($"⚠ Duplicate preset: {a} and {b} set the same value");
                        ConflictCount++;
                    }
                }
            }

            StatusText = ConflictCount == 0
                ? "No conflicts detected. Your flags look clean!"
                : $"Found {ConflictCount} potential conflict(s). Review the list above.";
        }
    }
}

using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NexusStrap.UI.ViewModels.Settings
{
    public class AntiCheatViewModel : NotifyPropertyChangedViewModel
    {
        public ICommand RefreshCommand { get; }

        private string _statusText = "Click Scan to check your flags against known anti-cheat triggers.";
        public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(nameof(StatusText)); } }

        private int _warningsFound;
        public int WarningsFound { get => _warningsFound; set { _warningsFound = value; OnPropertyChanged(nameof(WarningsFound)); OnPropertyChanged(nameof(SummaryText)); } }

        private int _safeFlags;
        public int SafeFlags { get => _safeFlags; set { _safeFlags = value; OnPropertyChanged(nameof(SafeFlags)); OnPropertyChanged(nameof(SummaryText)); } }

        public string SummaryText => $"Safe: {SafeFlags} | Warnings: {WarningsFound}";

        public ObservableCollection<string> WarningMessages { get; set; } = new();
        public ObservableCollection<string> SafeMessages { get; set; } = new();

        private static readonly (string Pattern, string Reason, string Severity)[] DangerousFlags = new[]
        {
            ("FFlagDebugDisableTelemetryEphemeral", "May trigger anti-cheat detection (telemetry tampering)", "High"),
            ("FFlagDebugDisableTelemetryEphemeralStat", "May trigger anti-cheat detection (telemetry tampering)", "High"),
            ("DFFlagDisableTelemetryEphemeralStat", "May trigger anti-cheat detection (telemetry tampering)", "High"),
            ("FFlagDebugDisableTelemetryEphemeralVariable", "May trigger anti-cheat detection (telemetry tampering)", "Medium"),
            ("FFlagDebugForceFSMCPULoopbackSignalCrit", "Debug flag that may trigger detection in production", "High"),
            ("FFlagDebugRenderingSignalCrit", "Debug rendering flag may trigger detection", "Medium"),
            ("DFIntDebugFPSCap", "FPS cap override may be detected by Byfron", "Low"),
            ("DFFlagDebugDrawSphere", "Debug draw flags should not be active in production", "Low"),
            ("DFFlagDebugDrawCircle", "Debug draw flags should not be active in production", "Low"),
            ("FFlagGameBasicSettingsFramerateCap", "Direct framerate cap may be monitored", "Low"),
            ("DFIntTaskSchedulerTargetFps", "FPS target override may trigger behavior analysis", "Low"),
            ("FFlagClientEnableMouseUnlockEvent", "Mouse unlock can be flagged by anti-cheat", "High"),
            ("FFlagDebugSimulateClientLuaTimeout", "Simulated timeouts may trigger detection", "High"),
            ("FFlagDebugSimulateLuaTimeout", "Simulated timeouts may trigger detection", "High"),
            ("DFFlagAudioRemoveAllEffects", "Audio manipulation may trigger integrity checks", "Medium"),
            ("DFFlagDebugRestrictGodrayEffect", "Rendering restrictions may trigger visual integrity checks", "Medium"),
        };

        public AntiCheatViewModel()
        {
            RefreshCommand = new RelayCommand(ScanFlags);
        }

        private void ScanFlags()
        {
            WarningMessages.Clear();
            SafeMessages.Clear();
            WarningsFound = 0;
            SafeFlags = 0;

            var allFlags = App.FastFlags.GetAllFlags().Select(f => f.Name).ToList();

            foreach (var (pattern, reason, severity) in DangerousFlags)
            {
                bool found = allFlags.Any(f => f.Contains(pattern, StringComparison.OrdinalIgnoreCase));
                if (found)
                {
                    WarningMessages.Add($"[{severity}] {pattern}: {reason}");
                    WarningsFound++;
                }
                else
                {
                    SafeFlags++;
                }
            }

            StatusText = WarningsFound == 0
                ? "All clear! No anti-cheat risky flags detected."
                : $"Found {WarningsFound} flag(s) that may trigger anti-cheat detection.";
        }
    }
}

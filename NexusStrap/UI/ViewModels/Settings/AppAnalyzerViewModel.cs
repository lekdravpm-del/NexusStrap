using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace NexusStrap.UI.ViewModels.Settings
{
    /// <summary>
    /// Combined diagnostics page: anti-cheat flag scan, flag conflict detection
    /// and a full environment health check, all in one place.
    /// </summary>
    public class AppAnalyzerViewModel : NotifyPropertyChangedViewModel
    {
        public ICommand ScanAntiCheatCommand { get; }
        public ICommand ScanConflictsCommand { get; }
        public ICommand RunHealthCheckCommand { get; }

        // Anti-cheat scan
        private string _antiCheatStatusText = "Click Scan Flags to check against known anti-cheat triggers.";
        public string AntiCheatStatusText { get => _antiCheatStatusText; set { _antiCheatStatusText = value; OnPropertyChanged(nameof(AntiCheatStatusText)); } }

        public ObservableCollection<string> WarningMessages { get; set; } = new();
        public ObservableCollection<string> SafeMessages { get; set; } = new();

        // Conflict scan
        private string _conflictStatusText = "Click Scan for Conflicts to check flag pairs.";
        public string ConflictStatusText { get => _conflictStatusText; set { _conflictStatusText = value; OnPropertyChanged(nameof(ConflictStatusText)); } }

        public ObservableCollection<string> Conflicts { get; set; } = new();

        // Health check
        private string _checkResults = "Click Run Health Check to diagnose your environment.";
        public string CheckResults { get => _checkResults; set { _checkResults = value; OnPropertyChanged(nameof(CheckResults)); } }

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

        public AppAnalyzerViewModel()
        {
            ScanAntiCheatCommand = new RelayCommand(ScanAntiCheatFlags);
            ScanConflictsCommand = new RelayCommand(ScanConflicts);
            RunHealthCheckCommand = new RelayCommand(RunHealthCheck);
        }

        private void ScanAntiCheatFlags()
        {
            WarningMessages.Clear();
            SafeMessages.Clear();

            var allFlags = App.FastFlags.GetAllFlags().Select(f => f.Name).ToList();
            int warningsFound = 0;
            int safeFlags = 0;

            foreach (var (pattern, reason, severity) in DangerousFlags)
            {
                bool found = allFlags.Any(f => f.Contains(pattern, StringComparison.OrdinalIgnoreCase));
                if (found)
                {
                    WarningMessages.Add($"[{severity}] {pattern}: {reason}");
                    warningsFound++;
                }
                else
                {
                    SafeMessages.Add($"{pattern}");
                    safeFlags++;
                }
            }

            AntiCheatStatusText = warningsFound == 0
                ? "All clear! No anti-cheat risky flags detected."
                : $"Found {warningsFound} flag(s) that may trigger anti-cheat detection.";
        }

        private void ScanConflicts()
        {
            Conflicts.Clear();

            var allFlags = App.FastFlags.GetAllFlags().ToDictionary(f => f.Name, f => f.Value);
            int conflictCount = 0;

            foreach (var (flagA, flagB, reason) in KnownConflicts)
            {
                if (allFlags.ContainsKey(flagA) && allFlags.ContainsKey(flagB))
                {
                    Conflicts.Add($"⚠ {flagA} ↔ {flagB}: {reason}");
                    conflictCount++;
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
                        conflictCount++;
                    }
                }
            }

            ConflictStatusText = conflictCount == 0
                ? "No conflicts detected. Your flags look clean!"
                : $"Found {conflictCount} potential conflict(s). Review the list above.";
        }

        private void RunHealthCheck()
        {
            var results = new System.Text.StringBuilder();
            int issuesFound = 0;
            int passedChecks = 0;

            CheckRobloxInstallation(results, ref issuesFound, ref passedChecks);
            CheckClientSettings(results, ref issuesFound, ref passedChecks);
            CheckFlagManager(results, ref issuesFound, ref passedChecks);
            CheckNetwork(results, ref issuesFound, ref passedChecks);
            CheckPaths(results, ref issuesFound, ref passedChecks);
            CheckPermissions(results, ref issuesFound, ref passedChecks);

            results.AppendLine();
            results.AppendLine($"Checks passed: {passedChecks} | Issues: {issuesFound}");

            CheckResults = results.ToString();
        }

        private void CheckRobloxInstallation(System.Text.StringBuilder results, ref int issuesFound, ref int passedChecks)
        {
            string robloxPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox");
            if (System.IO.Directory.Exists(robloxPath))
            {
                var versions = System.IO.Directory.GetDirectories(System.IO.Path.Combine(robloxPath, "Versions"));
                if (versions.Length > 0)
                {
                    results.AppendLine("✓ Roblox is installed with " + versions.Length + " version(s)");
                    passedChecks++;
                }
                else
                {
                    results.AppendLine("✗ Roblox Versions directory is empty");
                    issuesFound++;
                }
            }
            else
            {
                results.AppendLine("✗ Roblox installation not found");
                issuesFound++;
            }
        }

        private void CheckClientSettings(System.Text.StringBuilder results, ref int issuesFound, ref int passedChecks)
        {
            string clientSettings = System.IO.Path.Combine(Paths.Base, "ClientSettings", "ClientAppSettings.json");
            if (System.IO.File.Exists(clientSettings))
            {
                var info = new System.IO.FileInfo(clientSettings);
                results.AppendLine("✓ FFlag file exists (" + info.Length + " bytes, modified " + info.LastWriteTime.ToString("MMM dd HH:mm") + ")");
                passedChecks++;
            }
            else
            {
                results.AppendLine("✗ FFlag file (ClientAppSettings.json) not found");
                issuesFound++;
            }
        }

        private void CheckFlagManager(System.Text.StringBuilder results, ref int issuesFound, ref int passedChecks)
        {
            try
            {
                var flags = App.FastFlags.GetAllFlags();
                results.AppendLine("✓ FastFlagManager loaded with " + flags.Count() + " flags");
                passedChecks++;
            }
            catch (Exception ex)
            {
                results.AppendLine("✗ FastFlagManager error: " + ex.Message);
                issuesFound++;
            }
        }

        private void CheckNetwork(System.Text.StringBuilder results, ref int issuesFound, ref int passedChecks)
        {
            try
            {
                using var client = new System.Net.Http.HttpClient();
                client.Timeout = TimeSpan.FromSeconds(5);
                var response = client.GetAsync("https://roblox.com").Result;
                results.AppendLine("✓ Network connectivity OK (status: " + response.StatusCode + ")");
                passedChecks++;
            }
            catch
            {
                results.AppendLine("✗ Network connectivity issue - cannot reach roblox.com");
                issuesFound++;
            }
        }

        private void CheckPaths(System.Text.StringBuilder results, ref int issuesFound, ref int passedChecks)
        {
            if (Paths.Initialized)
            {
                results.AppendLine("✓ NexusStrap paths initialized (" + Paths.Base + ")");
                passedChecks++;
            }
            else
            {
                results.AppendLine("✗ NexusStrap paths not initialized");
                issuesFound++;
            }
        }

        private void CheckPermissions(System.Text.StringBuilder results, ref int issuesFound, ref int passedChecks)
        {
            try
            {
                string testFile = System.IO.Path.Combine(Paths.Base, "write_test.tmp");
                System.IO.File.WriteAllText(testFile, "test");
                System.IO.File.Delete(testFile);
                results.AppendLine("✓ Write permissions OK");
                passedChecks++;
            }
            catch
            {
                results.AppendLine("✗ Write permission issue in NexusStrap directory");
                issuesFound++;
            }
        }
    }
}
using System.Collections.ObjectModel;

namespace NexusStrap.Models.Persistable
{
    public class Settings
    {

        // Integration Page
        public bool EnableActivityTracking { get; set; } = true;
        public bool ShowServerDetails { get; set; } = true;
        public bool ShowServerUptime { get; set; } = false;
        public bool AutoRejoin { get; set; } = true;
        public bool ShowGameHistoryMenu { get; set; } = true;
        public bool PlaytimeCounter { get; set; } = true;
        public TrayDoubleClickAction DoubleClickAction { get; set; } = TrayDoubleClickAction.ServerInfo;
        public bool UseDisableAppPatch { get; set; } = false;
        public bool ShowUsingNexusStrapRPC { get; set; } = true;
        public bool UseDiscordRichPresence { get; set; } = true;
        public bool HideRPCButtons { get; set; } = true;
        public bool EnableCustomStatusDisplay { get; set; } = true;
        public bool ShowAccountOnRichPresence { get; set; } = true;
        public bool StudioRPC { get; set; } = false;
        public bool StudioThumbnailChanging { get; set; } = false;
        public bool StudioEditingInfo { get; set; } = false;
        public bool StudioWorkspaceInfo { get; set; } = false;
        public bool StudioShowTesting { get; set; } = false;
        public bool StudioGameButton { get; set; } = false;
        public ObservableCollection<CustomIntegration> CustomIntegrations { get; set; } = new();

        // Bootstrapper Page
        public bool ConfirmLaunches { get; set; } = true;
        public bool AllowCookieAccess { get; set; } = false;
        public bool AutoCloseCrashHandler { get; set; } = true;
        public CleanerOptions CleanerOptions { get; set; } = CleanerOptions.Never;
        public List<string> CleanerDirectories { get; set; } = new List<string>();
        public bool BackgroundUpdatesEnabled { get; set; } = false;
        public bool MultiInstanceLaunching { get; set; } = true;
        public bool Error773Fix { get; set; } = true;
        public int MultibloxInstanceCount { get; set; } = 2;
        public int MultibloxDelayMs { get; set; } = 1500;
        // AboveNormal can make the desktop, browser, and Discord feel worse on busy PCs.
        // Let Windows schedule Roblox normally unless the user explicitly opts in.
        public ProcessPriorityOption SelectedProcessPriority { get; set; } = ProcessPriorityOption.Normal;

        // FastFlag Editor/Settings Related
        public bool UseFastFlagManager { get; set; } = true;
        public bool ShowPresetColumn { get; set; } = false;
        public bool ShowFlagCount { get; set; } = true;
        public bool UseAltManually { get; set; } = true;

        // Appearance Page
        public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.NexusStrapDialog;
        public BootstrapperIcon BootstrapperIcon { get; set; } = BootstrapperIcon.IconNexus;
        public WindowsBackdrops SelectedBackdrop { get; set; } = WindowsBackdrops.Mica;
        public string Locale { get; set; } = "nil";
        public string? SelectedCustomTheme { get; set; } = null;
        public List<GradientStops> CustomGradientStops { get; set; } = new()
        {
            new GradientStops { Offset = 0.0, Color = "#4D5560" },
            new GradientStops { Offset = 0.5, Color = "#383F47" },
            new GradientStops { Offset = 1.0, Color = "#252A30" }
        };
        public double GradientAngle { get; set; } = 0;
        public BackgroundMode BackgroundType { get; set; } = BackgroundMode.Gradient;
        public string? BackgroundImagePath { get; set; }
        public BackgroundStretch BackgroundStretch { get; set; } = BackgroundStretch.UniformToFill;
        public double BackgroundOpacity { get; set; } = 1.0;
        public string BootstrapperTitle { get; set; } = App.ProjectName;
        public string BootstrapperIconCustomLocation { get; set; } = "";
        public string DownloadingStringFormat { get; set; } = Strings.Bootstrapper_Status_Downloading + " {0} - {1}MB / {2}MB";
        public Theme Theme { get; set; } = Theme.Dark;
        public string? CustomFontPath { get; set; } = null;

        // Settings Page
        public bool CheckForUpdates { get; set; } = true;
        public bool CheckForPreRelease { get; set; } = false;
        public bool WPFSoftwareRender { get; set; } = false;
        public bool UpdateRoblox { get; set; } = true;
        public string RobloxDomain { get; set; } = RobloxInterfaces.Deployment.DefaultRobloxDomain;
        public bool StaticDirectory { get; set; } = false;
        public string Channel { get; set; } = RobloxInterfaces.Deployment.DefaultChannel;
        public ChannelChangeMode ChannelChangeMode { get; set; } = ChannelChangeMode.Prompt;

        // Custom Launch Sound
        public bool EnableLaunchSound { get; set; } = false;
        public string? LaunchSoundPath { get; set; } = null;
        public double LaunchSoundVolume { get; set; } = 0.5;

        // Auto Region Select
        public bool AutoSelectBestRegion { get; set; } = false;
        public string ForcedRegion { get; set; } = "None";

        // Performance Overlay
        public bool EnablePerformanceOverlay { get; set; } = false;
        public int OverlayOpacity { get; set; } = 80;

        // Misc Stuff
        public bool IsNavigationSidebarExpanded { get; set; } = true;
        public string SelectedRegion { get; set; } = string.Empty;
        public bool ForceLocalData { get; set; } = false;
        public bool DebugDisableVersionPackageCleanup { get; set; } = false;

        // Custom Launch Arguments
        public ObservableCollection<CustomLaunchArg> CustomLaunchArgs { get; set; } = new();

        // Crash Recovery
        public bool CrashRecoveryEnabled { get; set; } = true;
        public int CrashRecoveryMaxRetries { get; set; } = 3;
        public int CrashRecoveryDelayMs { get; set; } = 2000;

        // Auto-Update FFlag Profiles
        public bool AutoUpdateFFlagsOnRobloxUpdate { get; set; } = false;
        public string? AutoUpdateFFlagTemplateName { get; set; } = null;

        // Discord RPC Customize
        public bool CustomRPCEnabled { get; set; } = false;
        public string? CustomRPCDetails { get; set; } = null;
        public string? CustomRPCState { get; set; } = null;

        // New Features
        // Smooth Window Dragging
        public bool EnableSmoothWindowDrag { get; set; } = false;

        // Custom Window Title
        public string CustomRobloxTitle { get; set; } = "";
        public string CustomRobloxLogoPath { get; set; } = "";

        // Profile Display
        public bool EnableProfileDisplay { get; set; } = true;

        // Start Menu Tile
        public bool EnableStartMenuTile { get; set; } = false;
        public string StartMenuTileArgs { get; set; } = "";

        // Server Browser with Filters
        public bool EnableServerBrowser { get; set; } = true;
        public ServerFilter SelectedServerFilter { get; set; } = ServerFilter.All;
        public string ServerPlayerCountFilter { get; set; } = "";
        public string ServerUptimeFilter { get; set; } = "";
        public string ServerRegionFilter { get; set; } = "";

        // Server Capacity Indicator
        public bool EnableServerCapacityIndicator { get; set; } = true;

        // Multi-language Support
        public ObservableCollection<string> EnabledLanguages { get; set; } = new()
        {
            "en",
            "es",
            "fr",
            "de",
            "it",
            "pt-BR",
            "ru",
            "ja",
            "ko",
            "zh-CN",
            "zh-TW",
            "ar",
            "he",
            "fa",
            "tr",
            "nl",
            "pl",
            "sv",
            "no",
            "da",
            "fi",
            "cs",
            "hu",
            "ro",
            "bg",
            "hr",
            "et",
            "lt",
            "sl",
            "lv",
            "eu",
            "bn",
            "hi",
            "th",
            "vi",
            "id",
            "ms",
            "tl"
        };

        // Friend System
        public bool EnableFriendSystem { get; set; } = true;
        public string FriendSearchFilter { get; set; } = "";

        // Auto-close on Roblox Launch
        public bool AutoCloseSettingsOnLaunch { get; set; } = false;

        // Performance Overlay Settings
        public bool EnablePingOverlay { get; set; } = true;
        public bool EnableFPSOverlay { get; set; } = true;
        public bool EnableRAMOverlay { get; set; } = true;
        public bool EnableCPUOverlay { get; set; } = true;
        public OverlayPosition OverlayPosition { get; set; } = OverlayPosition.TopLeft;
        public double OverlaySpacing { get; set; } = 10.0;

        // New Feature
        public bool EnableExtraFeature { get; set; } = false;

        // Memory Limiter / RAM Cleaner
        public bool EnableMemoryLimiter { get; set; } = false;
        public int MemoryLimitMB { get; set; } = 4096;
        public bool EnableRamCleaner { get; set; } = false;
        public int RamCleanIntervalMinutes { get; set; } = 10;
    }
}

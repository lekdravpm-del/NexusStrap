# NexusStrap — Comprehensive Source Analysis

Comprehensive documentation of the NexusStrap Roblox bootstrapper codebase (C# / WPF / Wpf.Ui).

- **Author/owner**: `lekdravpm-del` (`NexusStrap` repo)
- **Stack**: .NET (net8+; CI uses .NET 10 preview), WPF, Wpf.Ui 3.x (vendored fork in `wpfui\` submodule — excluded from this analysis), Windows only
- **License**: AGPL-3.0-or-later (headers on source files), plus LICENSE-MIT / LICENSE-UNLICENSE
- **Scope**: everything under `NexusStrap\` except `bin\`, `obj\`, and the `wpfui\` submodule

---

## 1. Application Overview

NexusStrap is a full Roblox "bootstrapper" (installer + launcher) with a large feature surface:

- Installs / updates Roblox Player and Studio from Roblox's own CDN (zip packages from Roblox's package maps, extracted with SharpZipLib `FastZip`).
- Tray-resident watcher while Roblox is running (game join detection, server info, RPC, crash recovery).
- Settings app (WPF, Wpf.Ui) with ~20 pages; FastFlag (ClientAppSettings) editing; account manager with DPAPI-encrypted alt accounts; Discord Rich Presence; localization in 30 languages; custom themes/backdrops; start-menu shortcut creation; region-based server joining; community mods; GBS (GameBootstrapperSettings) editing; analytics/health-check pages; and more.

### Process model

| Process | Purpose |
|---|---|
| `NexusStrap.exe` (bootstrapper mode) | Downloads/updates Roblox then launches the game (`-player` / `-studio` / `-studioauth`) |
| `NexusStrap.exe -watcher <base64 WatcherData>` | Spawned while Roblox runs; holds tray icon, activity tracking, RPC |
| `NexusStrap.exe -settings` | Settings window (also the main installed app) |
| `RobloxPlayerBeta.exe` / `RobloxStudioBeta.exe` | The game |

---

## 2. Project Layout (core folders)

```
NexusStrap/
├── App.xaml / App.xaml.cs        App entry, constants, startup, exception handling
├── Bootstrapper.cs               Install/launch flow (~2072 lines)
├── Installer.cs                  Self-install/uninstall, upgrade handling
├── LaunchHandler.cs              -player/-studio/-settings/-uninstall/... argument routing
├── LaunchSettings.cs             Command-line flags parsing
├── Watcher.cs                    Tray watcher lifecycle (watcher mode)
├── MultiInstanceWatcher.cs       ROBLOX_singletonMutex holder while launching
├── FastFlagManager.cs            ClientAppSettings.json editing (with undo/redo)
├── FFlagTemplateManager.cs       FFlag templates
├── GBSEditor.cs                  GameBootstrapperSettings.json editing
├── CookiesManager.cs             .ROBLOSECURITY cookie read from Roblox LocalStorage
├── JsonManager.cs / GlobalCache.cs / RemoteData.cs / HttpClientLoggingHandler.cs
├── Locale.cs                     Locale selection (30+ languages, RTL support)
├── Paths.cs                      Install paths, base dir, temp, start menu
├── Logger.cs / Utilities.cs / Resource.cs / StudioPluginManager.cs
├── Enums/                        LaunchMode, Theme, ServerType, FlagPresets, ChannelChangeMode...
├── Models/                       APIs (GithubRelease...), Entities (ActivityData...), Persistable (Settings, State), Manifest
├── Integrations/                 AccountManager, ActivityWatcher, RobloxServerFetcher,
│                                 PlayerDiscordRichPresence, NexusStrapRichPresence, StudioRPC, Watchers
├── Resources/                    Strings.resx + 30 locale resx, fonts (Rubik, Orbitron), icons,
│                                 bootstrapper styles, mods (cursor/sounds), power plans, preset flags
├── RobloxInterfaces/             Deployment, PackageManifest, RobloxPlayerData/RobloxStudioData
└── UI/
    ├── Elements/                 Bootstrapper dialogs, Settings MainWindow, AccountManager,
    │                             ContextMenu windows, Overlay, Base (WpfUiWindow)
    ├── ViewModels/               Bootstrapper, Settings, AccountManagers, ContextMenu...
    ├── Converters/ Style/ Utility/  (Converters, Frontend, NotifyIconWrapper, Shortcut, Win32WindowHelper...)
```

---

## 3. Startup & Lifecycle (`App.xaml.cs`)

- **Constants** (`App.xaml.cs:22-39`): `ProjectName = "NexusStrap"` (`"NexusStrap-QA"` under `#if QA_BUILD`), `ProjectOwner = "lekdravpm-del"`, `ProjectRepository = "lekdravpm-del/NexusStrap"`, `RobloxPlayerAppName = "RobloxPlayerBeta.exe"`, `RobloxStudioAppName = "RobloxStudioBeta.exe"`, `UninstallKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\{ProjectName}"`.
- **Singletons** (`App.xaml.cs:61-81`): `Logger`, `PendingSettingTasks` (dictionary of `BaseTask`), `Settings`/`State` (JsonManager), `PlayerState`/`StudioState` (LazyJsonManager<DistributionState>), `RemoteData`, `FastFlags`, `GlobalSettings` (GBSEditor), `Cookies`, `HttpClient` with logging handler.
- **OnStartup** (`App.xaml.cs:253-438`):
  1. `Locale.Initialize()`, apply saved custom font, log version/build metadata (embedded `BuildMetadataAttribute`), build User-Agent.
  2. Read `InstallLocation` from uninstall registry key; if the old user path differs (user rename) or the dir only holds `Settings.json`+`State.json` → implicit reinstall (`Installer.DoInstall`). If no install → `LaunchHandler.LaunchInstaller()`.
  3. Installed: `Paths.Initialize`, self-heal missing payload files (framework-dependent install), `Logger.Initialize` (fails on duplicate launch), then `RemoteData.LoadData` (async), `Settings/State/FastFlags/GlobalSettings.Load()`.
  4. Safety fixes: reset invalid `Theme` (>Custom → Dark), load cookies if `AllowCookieAccess`, reset invalid `Locale` to `"nil"`, `Locale.Set(locale)`.
  5. `Installer.HandleUpgrade()` (unless `-bypassupdate`), `WindowsRegistry.RegisterApis()`, `LaunchHandler.ProcessLaunchArgs()`.
- **Backdrops**: `App.WindowsBackdrop()` maps `Settings.Prop.SelectedBackdrop` (None/Mica/Acrylic/Aero) onto every `UiWindow` (Mica = normal window; Acrylic/Aero set `AllowsTransparency` + `WindowStyle=None`) (`App.xaml.cs:154-189`).
- **Exception handling**: `DispatcherUnhandledException` → `FinalizeExceptionHandling` → error taskbar state on the bootstrapper dialog + exception dialog + `Terminate(ERROR_INSTALL_FAILURE)` (`App.xaml.cs:104-142`).
- **OnExit**: `AccountManager.Shared.SaveAccounts()`, dispose RPC (`App.xaml.cs:440-445`).
- `App.xaml`: Wpf.Ui Dark theme + merged `UI/Style/Dark.xaml`, `NexusUnique.xaml`, `Default.xaml` (order matters for `WpfUiWindow.ApplyTheme`), Rubik font resources, `ShutdownMode=OnExplicitShutdown`.

---

## 4. Bootstrapper (`Bootstrapper.cs`, ~2072 lines)

- Modes via `LaunchMode` (Player / Studio / StudioAuth), storage classes `RobloxPlayerData` / `RobloxStudioData` (RobloxInterfaces).
- **Install flow** (`Run()` at line 188+): connectivity check → optional update check (`CheckForUpdates`) → mutex (`"NexusStrap-Bootstrapper"`) → waits for other instances → determines `_mustUpgrade` (force flag, state `ForceReinstall`, missing version GUID or executable) → download package zips from Roblox CDN (package maps from `RemoteData`), track `_totalDownloadedBytes` → `FastZip` extraction (`_fastZipEvents` cancel on `CancellationToken`) → registry client location registration.
- **Progress**: `ProgressBarMaximum = 10000`, taskbar progress capped at 1 (WPF) — `UpdateProgressBar()` clamps both (`Bootstrapper.cs:134-153`).
- **Connectivity errors**: `HandleConnectionError` distinguishes Roblox-down vs. generic; offers skip/retry depending on whether upgrade is required (`Bootstrapper.cs:155-186`).
- **After launch**:
  - Watcher spawn: serialize `WatcherData { ProcessId, LogFile, AutoclosePids, LaunchMode }` to base64 arg `-watcher "..."` (`Bootstrapper.cs:1020-1040`).
  - **Custom window title/icon** (`Bootstrapper.cs:1042-1047`):
    ```csharp
    _ = Task.Run(async () => {
        await Task.Delay(2000);
        Win32WindowHelper.ApplyWindowTitleAndIcon(_appPid, "Nexus");
    });
    ```
    Hardcoded `"Nexus"` — the `CustomRobloxTitle` setting (Settings.cs) is NOT read here.
  - `Cancel()` cleans up registry + partial install, or kills the game process (`Bootstrapper.cs:1071-1109`).
- `Win32WindowHelper.cs`: `FindWindowEx` over the PID's windows, `SetWindowText`, `SetClassLongPtr` (icon), `WM_SETICON` (0x0080) with `ICON_SMALL`/`ICON_BIG`.
- **Channels** (`Enums/ChannelChangeMode.cs`): `Automatic`, `Prompt`, `Ignore` — used by update/channel handling in the Settings app (ChannelPage); no per-channel Roblox install variants.

---

## 5. Watcher Mode (`Watcher.cs`, `UI/NotifyIconWrapper.cs`, `UI/Elements/ContextMenu/MenuContainer.xaml`)

`Watcher` (watcher process):
- Acquires named lock, `ActivityWatcher.Start()`, then **polls every 1s while the game PID lives** (`Watcher.cs:111-134`).
- **Crash recovery**: if `CrashRecoveryEnabled` and exit code ≠ 0, relaunch `Paths.Process -player` up to `CrashRecoveryMaxRetries` times with `CrashRecoveryDelayMs` between attempts (`Watcher.cs:136-167`).
- On exit: kills `AutoclosePids`, and if `TestModeFlag.Active` reopens settings with `-settings -testmode` (`Watcher.cs:169-177`).
- RPC wiring: Studio → `StudioRichPresence` when `StudioRPC`; Player → `PlayerRichPresence` when `UseDiscordRichPresence` (`Watcher.cs:72-75`).
- `UseDisableAppPatch` → kills Roblox main window when the Roblox app exits (`Watcher.cs:62-70`).
- `Dispose()`: cancels, multi-instance cleanup via `App.Bootstrapper.CleanupMultiInstanceResources()` when `MultiInstanceLaunching` (`Watcher.cs:188-192`).

`NotifyIconWrapper` (tray icon, `UI/NotifyIconWrapper.cs`):
- Icon `Properties.Resources.IconNexus`, text `NexusStrap`; right-click opens the `MenuContainer` context menu; left double-click runs `DoubleClickAction` (None / GameHistory / ServerInfo) with helpful hint boxes (`NotifyIconWrapper.cs:38-88`).
- **Join notifications**: on `OnGameJoin`, queries server location (`QueryServerLocation`, if `ShowServerDetails`) and uptime (`QueryServerTime`, if `ShowServerUptime`), localizes the text, shows a balloon tip that opens the Server Information window when clicked (`NotifyIconWrapper.cs:109-163`). Manual balloon API with click-handler replacement logic (`ShowAlert`).

`MenuContainer.xaml` (tray context menu):
- Header (version `NexusStrap v2.4.1` — note: hardcoded, xaml), disabled "Total playtime" item.
- `RichPresenceMenuItem` (checkable, toggle Discord RPC), `InviteDeeplinkMenuItem` (copy deeplink), `RegionMenuRoot` (Join Server for chosen region + preferred region ComboBox), `GameInformationMenuItem`, `ServerDetailsMenuItem`, `GameHistoryMenuItem`, `QuickLaunchRoot` (dynamically filled), `BookmarksRoot` + `BookmarkCurrentGameMenuItem`, "Close Roblox", "Close Watcher".
- Items shown conditionally (menu item `Visibility` toggled from code-behind based on settings: `ShowGameHistoryMenu`, `ShowServerDetails`, etc.).

---

## 6. Settings App (`UI/Elements/Settings/MainWindow.xaml`)

- `TitleBar` (Wpf.Ui), "NEXUS STRAP" brand header, Discord anchor link.
- `NavigationCompact` sidebar with pages:
  - Main: Integrations, Behaviour, FastFlags, FFlag Templates, Appearance, Region Selector, Roblox Settings (GBS), Shortcuts
  - Utility: Server History, Launch Arguments, Log Viewer, Anti-Cheat, Conflict Detector, Health Check, Analytics
  - Hidden (Collapsed, toggled from code): FastFlag Editor (+warning), Optimization Setup, Community Mods, Preset Mods, Mod Generator, Theme Store
  - Footer: Account Manager (command), Settings (ChannelPage), About
- Status bar: Test Mode toggle, "Made by Troll/Recd", Close / Save / Restart / Save & Launch (context menu: Launch Player / Launch Studio).
- Loading overlay with ProgressRing; `AlertBar` InfoBar bound to `RemoteData` alerts; Snackbars for "already running" and "settings saved" (`MainWindow.xaml.cs:105-109`, `RequestSaveNoticeEvent`).
- **Window state** persisted to `App.State.Prop.SettingsWindow` (size/position with virtual-screen bounds check) (`MainWindow.xaml.cs:83-103`).
- **Unsaved-changes guard** on close (`MainWindow.xaml.cs:127-144`): warns if `App.FastFlags.Changed || App.PendingSettingTasks.Any()`.
- On close: `TestModeFlag` → launch Roblox; otherwise `App.SoftTerminate()` (`MainWindow.xaml.cs:146-152`).
- First-run routing to `OptimizationSetupPage` when `ShowOptimizationSetup` (`MainWindow.xaml.cs:45-48`).

---

## 7. Settings & State Model (`Models/Persistable/Settings.cs`, `State.cs`)

Persisted as `Settings.json` / `State.json` in the install dir; loaded via `JsonManager<Settings>`.

### Implemented (wired) settings, by area
- **Behaviour/launch**: `StaticDirectory`, `ForceReinstall`, `CheckForUpdates`, `UpdateOnBootstrapperOpen`, `BootstrapperTitle`, `BootstrapperIcon` (enum → GetIcon), `ShowBootstrapperCancelButton`, `LaunchChannel`..., `LaunchSettings` (test mode, quiet, bypassupdate, uninstall, force, upgrade...)
- **Graphics/FastFlags**: `UseAltManually`, `EnableFPSUnlocker`, `FpsUnlockerValue`, plus the entire FastFlag preset surface (rendering modes D3D11/Vulkan/OpenGL, MSAA 1-8, quality 1-21, low-poly, remove grass, gray sky, disable DPI scale, pause voxelizer, manual fullscreen — `FastFlagManager.PresetFlags`, lines 22-59)
- **Tracking**: `EnableActivityTracking`, `ShowServerDetails`, `ShowServerUptime`, `ShowGameHistoryMenu`, `AutoJoinEnabled` (region auto-join), `EnableServerLocationAutoJoin`... 
- **RPC**: `UseDiscordRichPresence`, `StudioRPC`, `ShowRichPresencePlaytime`
- **Accounts**: `EnableAccountManager`, `AllowCookieAccess`, `RememberLastAltAccount`, `RememberLastAltAccountAsLaunchDefault`
- **App**: `Locale`, `Theme` (`Enums.Theme`: Dark/Default/NexusStrap/Custom), `SelectedBackdrop` (None/Mica/Acrylic/Aero), `CustomFontPath`, `DoubleClickAction`, `CrashRecoveryEnabled`, `CrashRecoveryMaxRetries`, `CrashRecoveryDelayMs`, `MultiInstanceLaunching`, `UseDisableAppPatch`
- **Tray/misc**: `ShowServerUptime`, `TrayTooltipBehavior`..., `AllowFastFlagEditor`, `HideConsole`, `StudioPluginSettings`...
- `State.cs`: `SettingsWindow` bounds, `LastPage` (FullName of last navigation target), `ForceReinstall`, `ShowOptimizationSetup`, plus per-game `GamesHistory` (LastPlayedGuid, PlayCount, LastPlayedAt) and launch/account state.

### ⚠️ Declared but NOT wired anywhere (planned features)
From `Settings.cs:91-278`, grep-verified to have **zero usages** outside Settings.cs itself:

- `EnablePerformanceOverlay` — overlay actually exists but is hardcoded via a separate toggle in code-behind (`PerformanceOverlay` is only shown based on a menu toggle, not this setting)
- `EnableSmoothWindowDrag`
- `CustomRobloxTitle` (see §4 — hardcoded `"Nexus"`)
- `EnableProfileDisplay`
- `EnableStartMenuTile` (start-menu **shortcuts** exist via `ShortcutsPage` + `Utility/Shortcut.cs`; a "tile" toggle does not)
- `EnableServerBrowser` + `SelectedServerFilter`, `ServerPlayerCountFilter`, `ServerUptimeFilter`, `ServerRegionFilter` (server **browser** UI does not exist; region join exists)
- `EnableServerCapacityIndicator` (no capacity/stair indicators in `ServerInformation.xaml` — see §10)
- `EnableFriendSystem`

`README_FEATURE_IMPLEMENTATION.md` confirms these are roadmap items, not implemented features.

---

## 8. UI Skins — Bootstrapper Dialogs

| Dialog | File | Notes |
|---|---|---|
| `NexusStrapDialog` (default) | `UI/Elements/Bootstrapper/NexusStrapDialog.xaml` (+`.cs`) | `520x300`, `WindowStyle=None`, `ExtendsContentIntoTitleBar`, custom dark gradient + decorative ellipses, `ui:TitleBar` (no buttons), `ui:TitleBar` row with icon + "NEXUS STRAP" Orbitron text, status message, Cancel button, thin ProgressBar, TaskbarItemInfo progress |
| `FluentDialog` | `UI/Elements/Bootstrapper/FluentDialog.xaml` | WPF-styled variant |
| WinForms variants | `Resources/BootstrapperStyles/` (`ByfronDialog`, `TwentyFiveDialog`) | `WinFormsDialogBase` constants for taskbar progress |
| `FullQuality` MessageBox | `Resources/MessageBox/FullQuality/` | Custom exception/connectivity dialogs |

`NexusStrapDialog.xaml.cs` implements `IBootstrapperDialog`: message/progress/taskbar-progress/cancel props; closing → `Bootstrapper.Cancel()`; `ShowSuccess` via `BaseFunctions`.

---

## 9. Account Manager (`Integrations/AccountManager.cs`, `UI/Elements/AccountManager/`)

- **AltAccount record**: `SecurityToken`, `UserId`, `Username`, `DisplayName`; stored in `AccountManager.json` (base dir) **encrypted with DPAPI** (entropy `"NexusStrap_DPAPI_v1"`).
- **Add flows**: manual add, **quick sign-in** (`AddAccountByQuickSignInAsync` — token from the logged-in Roblox player process; emits `QuickSignCodeCreated`), and **browser login** (`AddAccountByBrowser` — PuppeteerSharp + ExtraStealth to get `.ROBLOSECURITY`).
- **Validation**: `ValidateAllAccountsAsync` pings Roblox's authenticated user endpoint; `CookiesManager` also parses the `.ROBLOSECURITY` out of Roblox's LocalStorage (`RobloxCookies.dat`) when `AllowCookieAccess`.
- **Account switching/launch**: `SetActiveAccount` raises `ActiveAccountChanged`; `LaunchAccount…` passes security tokens to the player; quick-switch UI in the Account Manager window.
- AccountManager window: `UI/Elements/AccountManager/MainWindow.xaml` — `Title="Nexus Strapper"`, `ui:TitleBar`, accounts grid, `Pages/FriendsPage.xaml` + `Pages/GamesPage.xaml` (navigation list).
- Friends/Games pages are **disabled until an account is active** (`MainWindow.xaml.cs` `UpdateNavigationItemsState`).
- Placeholder line 56 in `MainWindow.xaml`: `ToolTip="Nexus Strapper Profile"` block referencing `/NexusStrap;component/Assets/appicon.ico` and `/Assets/user.png` — **the `Assets/` folder does not exist in the repo** (broken placeholder references; the window still compiles because the XAML resources are only resolved at runtime).

### Friends (`UI/ViewModels/AccountManagers/FriendsViewModel.cs`)
- Sources friends from the **Roblox API** (`GET /v1/users/{id}/friends` + bulk presence/avatar calls using `Cookies.AuthGet`), NOT from custom IDs.
- `FriendInfo(long Id, string DisplayName, string? AvatarUrl, int PresenceType, string LastLocation, string StatusColor, string PlayingGameName)`; `IsOnline => PresenceType == 2` (Roblox presence enum).
- Filters: `All / Studio / Online / Website / Offline` (player-filter enum in the page).

---

## 10. Activity Tracking & Server Info

### `Integrations/ActivityWatcher.cs`
- Tail-parses Roblox `FLog` output (`[FLog::Output]` lines) for: `! Joining game`, `[FLog::GameJoinUtil] GameJoinUtil::initiateTeleportToPlace`, `[FLog::Network] serverId:`, `UDMUX Address =`, custom `[NexusStrapRPC]` channel messages.
- Exposes `Data` (ActivityData), `InGame`, and events `OnGameJoin`, `OnGameLeave`, `OnAppClose`, `OnRPCMessage`, `OnStudioMessage`.
- Handles teleports (new place + server id) and tracks playtime.

### `Integrations/RobloxServerFetcher.cs`
- **IP → region mapping** for `128.116.x.0/24` blocks (LA, Frankfurt, Ashburn, Paris, Amsterdam, Atlanta, London, NYC, Miami, Singapore, San Jose, Chicago, Sydney, etc.) — sourced from BTRoblox `serverdetails.js` logic.
- `QueryServerLocation()` resolves a server ID to an IP (via Roblox public resolver), then maps to a region name.
- `QueryServerTime()` → server uptime.

### Server Information window (`UI/Elements/ContextMenu/ServerInformation.xaml`)
- Rows: **Server Type** (Public/Private/Reserved), **Instance ID** (with Copy button), **Location** (visible when `ShowServerDetails`), **Uptime** (visible when `ShowServerUptime`).
- **No player count / capacity indicator / "stair" graphic** — `EnableServerCapacityIndicator` is unwired (see §7).

### Game History (`ServerHistory.xaml`, `GameInformation.xaml`)
- `ServerHistory`: list of previously joined servers (instance id, place, time, server type) with re-join ("Join Last Server") — from `ActivityWatcher` history store.
- `GameInformation`: current game details + quick-launch/bookmark actions.

---

## 11. Discord Rich Presence

| RPC | Client ID | Scope |
|---|---|---|
| `PlayerDiscordRichPresence.cs` | `1005469189907173486` | In-game presence (game icon, elapsed, server, account) |
| `NexusStrapRichPresence.cs` | `1534122334670159892` | Bootstrapper/settings presence; tracks pages & dialogs, `SetDialog("Account Manager")`, `SkipIdenticalPresence=true` |

- Studio RPC exists too (`Models/NexusStrapRPC/StudioRPC`). `OnExit`/`Watcher.Dispose` dispose RPC.

---

## 12. FastFlags (`FastFlagManager.cs`, `FFlagTemplateManager.cs`, `GBSEditor.cs`)

- **`FastFlagManager`** (JsonManager\<Dictionary<string,object>>): writes `ClientSettings/ClientAppSettings.json` under the install base; `SetValue(key, null)` = delete; **undo/redo stacks** (`SaveUndoSnapshot` before each mutation, `DictionaryEquals` dedupe); `Changed` flag drives the unsaved-changes prompt.
- **Presets** (`PresetFlags`, `FastFlagManager.cs:22-59`): manual fullscreen, pause voxelizer, disable DPI scaling, texture quality override + FRM quality, 4× low-poly CSG distance flags, graphics mode (D3D11/Vulkan/OpenGL), gray sky, MSAA (1/2/4/8), remove grass (3 FInt flags). `SetPreset(prefix, value)` / `SetPresetEnum` (rendering mode, MSAA, quality 1–21 enums).
- Profiles: save/load named flag sets under `Profiles/` (`Paths.SavedFlagProfiles`); `DeleteProfile` on the manager.
- **`FFlagTemplateManager`**: template import/export from `Resources/PresetFlags/`.
- **`GBSEditor`** (`RobloxSettingsPage` / `GlobalSettings`): edits Roblox `GameBootstrapperSettings.json` (GBS) — guarded by `App.GlobalSettings.Loaded`; disabled state dims the nav item (`MainWindow.xaml.cs:37-38`).
- App writes flags to the **installed Roblox version dir** during bootstrapping; `UseAltManually` forces `Rendering.ManualFullscreen = False` on load (`FastFlagManager.cs:214-218`).

---

## 13. Localization (`Locale.cs` + `Resources/Strings*.resx`)

- **31 resx files** (default `Strings.resx` + 30 locales): ar, bg, cs, de, en-US, es-ES, fa, fi, fil, fr, hr, hu, id, it, ja, ko, lt, ms, nl, pl, pt-BR, ro, ru, sv-SE, th, tr, uk, vi, zh-CN, zh-TW.
- Generated accessor `Resources/Strings.Designer.cs` (~4700+ lines).
- `Locale.cs`: `SupportedLocales` dict (locale code → display name); QA builds include extra locales (sq, bn, bs, da, el, he, hi); **RTL list** `{ar, he, fa}` with `RightToLeft` flag; fallback to `nil`/English when the saved locale is missing (`App.xaml.cs:422-428`).
- Note: many strings in XAML/UI are **hardcoded English** (e.g. tray menu items, "NEXUS STRAP") — only a subset of strings go through `Strings.*`.

---

## 14. Shortcuts & Start Menu (`Utility/Shortcut.cs`, `ShortcutsPage.xaml`)

- `Shortcut.Create(exePath, exeArgs, lnkPath)` builds `.lnk` via COM `ShellLink`; result `GenericTriState`; failure → `Frontend.ShowMessageBox` warning.
- `ShortcutsPage` (Settings) lets users create/remove start-menu + desktop shortcuts for NexusStrap and Roblox modes.
- **`EnableStartMenuTile` (tile) is not wired** — only classic shortcuts are implemented.

---

## 15. Other Notable Components

- `Installer.cs`: self-install into `%LocalAppData%\NexusStrap` (framework-dependent payload: exe + dll + runtimeconfig), uninstall registry key, `HandleUpgrade()` comparing versions, `DeployApplicationPayload()` (self-heal), quiet uninstaller support (`-uninstall -quiet`).
- `LaunchHandler.cs`: routes `-player`, `-studio`, `-studioauth`, `-settings`, `-watcher`, `-uninstall`, `-install`, `-upgrade`, `-testmode`, `-bypassupdate`, `-force`, `-quiet`; implements `NextAction` queued commands (e.g., launch after install, reopen settings after channel change).
- `LaunchSettings.cs`: flag parser (`TestModeFlag`, `UninstallFlag`, `QuietFlag`, `ForceFlag`, `UpgradeFlag`, `RobloxLaunchArgs`...).
- `MultiInstanceWatcher.cs`: holds `ROBLOX_singletonMutex`, fires `NexusStrap-MultiInstanceWatcherInitialisationFinished` EventWaitHandle, exits when no Roblox/NexusStrap processes remain.
- `RemoteData.cs`: fetches remote config (alerts, package maps, presence settings) — bootstrapper waits for it before downloading.
- `CookiesManager.cs`: reads `.ROBLOSECURITY` from `LocalStorage/RobloxCookies.dat` (domain-aware file name via `Deployment.RobloxDomain`), used for authenticated API calls.
- `StudioPluginManager.cs`: Studio plugin install.
- `Resources/NexusStrapPowerPlans/`: power-plan management for optimization; `OptimizationSetupPage` on first run.
- `Resources/Mods/`: community mods (cursor, sounds) installed into Roblox content; `CommunityModsPage`, `ModsPresetsPage`, `ModGeneratorPage` (hidden nav items).
- `WindowsRegistry.cs` (RobloxInterfaces): client/install registration keys + `RegisterApis`.

---

## 16. Build, Release & Cleanup

- `.github/workflows/release.yml`: builds on .NET 10 (preview) SDK, `win-x64`, self-contained, single-file, `ReadyToRun`, PDB exclusion; renames artifact to `NexusStrap-<version>.exe`; publishes via `softprops/action-gh-release`; embed `BuildMetadataAttribute` (commit hash/ref/timestamp/machine).
- `app.manifest`: DPI awareness, UAC requestedExecutionLevel (asInvoker default), Windows 10+ compatibility.
- `cleanup-nexusstrap.ps1` (repo root): safe uninstaller — kills `NexusStrap`/`RobloxPlayerBeta` processes, runs `-uninstall -quiet` if present, removes `%LOCALAPPDATA%\NexusStrap` and `NexusStrap-QA` folders.
- `README_FEATURE_IMPLEMENTATION.md`: roadmap/implementation plan for the unwired features (see §7).

---

## 17. Feature Verification Summary (what the user asked about)

| Claimed feature | Status | Evidence |
|---|---|---|
| Smooth window drag | **Not implemented as a setting** | `EnableSmoothWindowDrag` in Settings.cs only; windows use Wpf.Ui `TitleBar` default drag |
| Custom Roblox title bar | **Partially** — hardcoded "Nexus" | `Bootstrapper.cs:1042-1047` + `Win32WindowHelper`; `CustomRobloxTitle` setting unused |
| Profile display in account manager | **Placeholder only** | AccountManager MainWindow.xaml line ~56; broken `/Assets/user.png` refs (no Assets folder) |
| Server browser with filters (player count/uptime/region) | **Not implemented** | `EnableServerBrowser` + 4 filter settings unused; only region join + history exist |
| Server capacity indicators | **Not implemented** | `EnableServerCapacityIndicator` unused; ServerInformation.xaml shows only type/id/location/uptime |
| Performance overlay (horizontal) | **Exists, vertical layout** | `UI/Elements/Overlay/PerformanceOverlay.xaml` (5-row vertical, 220×140, top-right, 1s timer); toggle in code-behind, not via the unused setting |
| Start menu tile | **Not implemented** (shortcuts exist) | `EnableStartMenuTile` unused; `Utility/Shortcut.cs` + ShortcutsPage only |
| All supported languages | **30 locales + default** | `Resources/Strings*.resx` (31 files), `Locale.SupportedLocales` |
| Friends with custom IDs | **No** — Roblox user IDs | `FriendsViewModel.FriendInfo.Id` (long) from Roblox API; no custom-ID concept |
| Remove open-source markers | **Not implemented** | No such code; AGPL headers, LICENSE files all present; `cleanup-nexusstrap.ps1` is only a safe uninstaller |

---

## 18. Known Quirks & Bugs Noted in Code

- `Bootstrapper.cs:1046` — hardcoded `"Nexus"` window title ignores the user setting.
- `Bootstrapper.cs:1038` — `if (ipl.IsAcquired || true)` (always-true condition; dead lock logic).
- `MainWindow.xaml.cs:37-38` — GBS enabled binding "doesn't work as expected", set in code-behind.
- `NotifyIconWrapper.cs:9` — author comment: "lol who needs properly structured mvvm and xaml".
- `MenuContainer.xaml:34` — hardcoded `NexusStrap v2.4.1` (version drift risk).
- `App.xaml.cs:410-417` — workaround comment for missing `ui/style/.xaml` resource in installer builds.
- `AccountManager/MainWindow.xaml` — references non-existent `/Assets/appicon.ico` and `/Assets/user.png` (runtime resource resolution, not compile-time).
- `Bootstrapper.cs:142-144` — "bugcheck: if we're restoring a file from a package, it'll incorrectly increment the progress beyond 100 — too lazy to fix properly so lol" (clamped).
- Friend page relies on the active account's cookie; when no account is active the page is disabled.

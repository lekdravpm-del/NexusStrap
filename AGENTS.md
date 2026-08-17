# AGENTS.md — NexusStrap

This file captures high-signal guidance for agents working on this repo.

## Quick commands

- **Build:** `dotnet build NexusStrap/NexusStrap.csproj -c Release`
- **Publish:** `dotnet publish NexusStrap/NexusStrap.csproj -c Release -r win-x64 -p:SelfContained=false -p:PublishSingleFile=true -p:PublishReadyToRun=false`
- **Copy exe:** `Copy-Item "NexusStrap\bin\Release\net10.0-windows\win-x64\publish\NexusStrap.exe" "NexusStrap.exe"`

## Git pitfalls

- **`git checkout -f main` overwrites all local changes.** If you need to switch branches, first stash or commit your work.
- The local copy was not a git repo (`no .git`); we initialized one and merged the safety backup branch.
- Always verify `git status --short` after git operations.

## Key project structure

- **Entry point:** `NexusStrap/App.xaml.cs` — launches the Settings window + bootstrapper.
- **Settings UI:** `NexusStrap/UI/Elements/Settings/MainWindow.xaml` + `.cs` — navigation, search, status bar.
- **Bootstrapper:** `NexusStrap/Bootstrapper.cs` — launch logic, FFlags, version directory cleanup.
- **Mods system:** `NexusStrap/Models/Entities/ModPresetFileData.cs`, `NexusStrap/Models/SettingTasks/EnumModPresetTask.cs` — uses `Paths.Modifications` (NOT `Paths.PresetModifications`).
- **FFlags:** `NexusStrap/FastFlagManager.cs` + `NexusStrap/FFlagTemplateManager.cs` — force-copies `ClientAppSettings.json` to `_latestVersionDirectory`.
- **Cursor types:** `NexusStrap/Enums/CursorType.cs` + resources in `NexusStrap.csproj` as `EmbeddedResource`.
- **Integrations:** `NexusStrap/UI/Elements/Settings/Pages/IntegrationsPage.xaml` — Force Region dropdown added.

## Build / publish quirks

- Must publish **without** `-o` flag, then copy the exe manually, or MC3074 XAML error occurs.
- `-p:PublishReadyToRun=false` is mandatory (true crashes locally).
- `dotnet build` must succeed before `dotnet publish`.

## Conventions & fixes we added

- **Force Region:** New setting `ForcedRegion` (None/US/EU/Asia/AU/SA/JP) in Integrations. When set, all game joins auto-select a server in that region.
- **Search bar:** In Settings header; typing "roblox" reveals a Secret easter-egg tab → rickroll.
- **OptimizationSetup:** Hidden from nav, only appears in search.
- **Old Death Sound:** Toggle in Mods page; sound is `OldDeath.ogg`.
- **3 new cursor types:** RedCross, NeonGreen, CyanDot — PNGs in `Resources/Mods/Cursor/`, added to enum + csproj resources.
- **MouseLockedCursor.png** added to all cursor presets.
- **Aurora + Neon themes removed** from bootstrapper style selections.
- **Gamer1 watermark removed** from Settings status bar.
- **Close button** moved next to Save in status bar (col 3 = Close, col 4 = Save).
- **Save & Launch** always closes the window after launch.
- **Shortcut.cs** now always deletes + recreates `.lnk` files (was returning early if already existed).
- **Emoji URL** changed from `nexusstraplabs` → `bloxstraplabs`.
- **Mods path bug fixed:** all paths changed from `Paths.PresetModifications` → `Paths.Modifications`.
- **EnumModPresetTask stream bug:** was opening `data.ResourceStream` twice; fixed to use `CopyTo`.
- **Second-pass mod scan:** Bootstrapper scans unregistered folders in `Modifications` root on second pass.
- **Hardware performance recommendation:** `PerformanceOptimizer.Apply()` applies FFlag templates based on tier (Ultra→"1300 FPS Ultra", High→"Bloxstrap Optimized", Mid→"Gamer Tested", Low→"Low End Savior").
- **Bootstrapper stays open fix:** `LaunchHandler.cs` uses `Dispatcher.InvokeAsync` for `CloseBootstrapper()` on UI thread + 500ms delay.
- **Settings closes after Save & Launch:** `CloseWindow()` moved outside if/else in `SaveAndLaunch`.
- **About page:** "Gamer1 is a femboy~" subtitle (9px, non-rotating) added.
- **Server Browser:** Removed from Account Manager nav; Games page restored.
- **About page:** Gamer1 subtitle added; "Made by Recd" replaced with empty text.

## Code-modifying gotchas

- **`MainWindow.xaml.cs` `_alwaysHiddenTags`:** Only `"fastflageditor"` and `"fastflageditorwarning"` are hard-hidden; everything else (including `"optimizationsetup"`) is searchable.
- **XAML name scope:** `x:Name` must be on the `OptionControl` element itself (MC3093).
- **NaN crash:** `PositionHighlight` uses `ActualWidth`/`ActualHeight` with NaN fallbacks; never use `.Width`/`.Height` directly on `HighlightBox`.
- **Bootstrapper close:** `Dialog?.CloseBootstrapper()` uses `Dispatcher.InvokeAsync` to marshal to UI thread.
- **Installer:** Publish without `-o` then copy exe; or MC3074 occurs.

## Things we tried & failed / learned

- `git checkout -f` wipes local source changes — always commit/safety-backup first.
- SpotlightGuide string resources (`Strings.Guide_*`) are ~60 entries; easiest to hardcode strings in code rather than add to `Strings.resx`.
- Cursor PNGs must be added to `NexusStrap.csproj` as `EmbeddedResource`; otherwise they won't be embedded.
- `PublishReadyToRun=false` is mandatory or it crashes on launch.
- The `_latestVersionDirectory` is found by scanning `%LocalAppData%\Roblox\Versions\` for `version-*` folders.
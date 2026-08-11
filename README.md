<h1 align="center">
    NexusStrap
</h1>

<p align="center">
    NexusStrap is a fork of FishStrap focused on performance and customization
</p>

<p align="center">
    If you want to help support our project please consider giving this repo a star!
</p>

> [!CAUTION]
> Only download NexusStrap from the official repository or releases provided by the project. Do not download binaries from unofficial sources, as they may be modified or malicious.

## Download

Grab the latest `NexusStrap.exe` from the repository's Releases page.

## Building from Source

Requirements: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

```bash
# Build only
dotnet build NexusStrap.slnx -c Release

# Publish single .exe (self-contained, ~95MB)
dotnet publish NexusStrap/NexusStrap.csproj -c Release -p:PublishProfile=Publish-x64
```

Output: `NexusStrap/bin/Release/net10.0-windows/publish/win-x64/NexusStrap.exe`

---

## Key Improvements Over NexusStrap

### Integrations

* Player and message logs are now combined into **Logs Menu**
* Disable Roblox’s built-in screenshot and video recording system
* Custom NexusStrap Discord RPC that shows the current page/dialog
* Replace "Playing Roblox" with the name of the game you're playing using Custom Status Display
* Game history logging is now toggleable
* PlayTime Counter shows both total and session playtime

### Bootstrapper

* Switch between all of the classic Roblox icons for the top bar icon
* Change the Roblox process priority
* Automatically close the Roblox Crash Handler to reduce memory usage
* Integrated cleaner tool to remove leftover files (feature was made by FishStrap first, not us)
* Multi-instance launching support

### Mods

* Generate Mods using custom gradient colors easily
* Quick use Custom Cursors and Custom Shiftlocks easily
* Have multiple Custom Cursor Sets ready for use with the click of a button
* Easily add custom death sounds

### FastFlag Enhancements

* Toggle advanced settings
* Better Profiles
* Press Ctrl+Z and Ctrl+Y to Undo/Redo changes
* Built-in FastFlag lists inside of the profiles dialog
* Remove invalid/default flags and automatically update outdated ones in one click
* Use Preset Column to help find which flags are toggleables in fastflag settings
* FastFlag Warning system to help tell you about ban worthy fastflags
* Select values quickly using the built-in value selector when adding flags
* Use Find Flag feature to check all of Roblox's FastFlags
* Publish and Use other peoples published lists in Public Flag Lists

### UI & Appearance

* Fully customizable bootstrapper launcher
* Change the app font to any font you want
* Supports animated GIF, image, and gradient backgrounds themes
* Built-in App themes
* Use element toolbox when creating custom launchers (probably removing soon because it's useless)

### Settings

* Disable Hardware Acceleration to lower NexusStrap memory usage
* Disable NexusStrap Animations to help with performance
* Fixed auto update that won't go off randomly
* Easily switch Roblox update channels with action presets
* Option to fully block Roblox updates
* Quickly Reset/Import/Export all your settings in one place
* Use the debug menu to read log files
* Easily uninstall NexusStrap if it's not to your liking

### Extra Features

* Remembers the last opened tab
* Import settings from other bootstrappers easily like FishStrap and NexusStrap
* Built-in PC tweaks for performance optimization
* Create game shortcuts for faster game joining
* Includes a hidden Easter egg page and game

More features are planned to be added. You can also suggest a feature in the Issues section.

---

## Licensing

This project is **dual-licensed** under `GPL-3.0-or-later` and `Unlicensed`.
